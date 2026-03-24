-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Kyle Crowder
-- Create date: 3/22/2026
-- Description:	
-- =============================================




ALTER PROCEDURE [dbo].[sp_Search_Rerank]
    @Query      NVARCHAR(4000),
    @Embedding  VECTOR(1536),
    @TopN       INT = 50      -- how many to pull from Stage 2
AS
BEGIN
    SET NOCOUNT ON;

    --------------------------------------------------------------------
    -- Stage 2: get hybrid-ranked candidates
    --------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#Stage2') IS NOT NULL DROP TABLE #Stage2;

    CREATE TABLE #Stage2
    (
        Id            UNIQUEIDENTIFIER,
        Content       NVARCHAR(MAX),
        Summary       NVARCHAR(MAX),
        Keywords      NVARCHAR(MAX),
        ft_rank       INT,
        bm25_score    FLOAT,
        vector_score  FLOAT,
        hybrid_score  FLOAT
    );

    INSERT INTO #Stage2 (Id, Content, Summary, Keywords, ft_rank, bm25_score, vector_score, hybrid_score)
    EXEC dbo.sp_Search_Hybrid
        @Query     = @Query,
        @Embedding = @Embedding,
        @TopN      = @TopN;

    --------------------------------------------------------------------
    -- Build JSON payload for reranker: query + candidates
    --------------------------------------------------------------------
    DECLARE @Payload NVARCHAR(MAX);

    SELECT @Payload =
        JSON_OBJECT(
            'query': @Query,
            'candidates': JSON_QUERY(
                (
                    SELECT
                        Id,
                        Content,
                        Summary
                    FROM #Stage2
                    FOR JSON PATH
                )
            )
        );

    --------------------------------------------------------------------
    -- Call external reranker (LLM / cross-encoder)
    -- Replace URL, headers, and response shape as needed.
    --------------------------------------------------------------------
    DECLARE @RerankResponse NVARCHAR(MAX);

    EXEC sys.sp_invoke_external_rest_endpoint
        @url    = N'https://your-reranker-endpoint/api/rerank',
        @method = N'POST',
        @headers = N'{"Content-Type":"application/json"}',
        @payload = @Payload,
        @response = @RerankResponse OUTPUT;

    --------------------------------------------------------------------
    -- Parse reranker response: expected shape:
    -- {
    --   "results": [
    --     { "id": "<guid>", "score": 0.987 },
    --     ...
    --   ]
    -- }
    --------------------------------------------------------------------
    IF OBJECT_ID('tempdb..#Rerank') IS NOT NULL DROP TABLE #Rerank;

    CREATE TABLE #Rerank
    (
        Id           UNIQUEIDENTIFIER,
        rerank_score FLOAT
    );

    INSERT INTO #Rerank (Id, rerank_score)
    SELECT
        TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(value, '$.id'))       AS Id,
        TRY_CONVERT(FLOAT,            JSON_VALUE(value, '$.score'))    AS rerank_score
    FROM OPENJSON(@RerankResponse, '$.results');

    --------------------------------------------------------------------
    -- Final output: join Stage 2 + reranker scores
    --------------------------------------------------------------------
    SELECT
        s.Id,
        s.Content,
        s.Summary,
        s.Keywords,
        s.ft_rank,
        s.bm25_score,
        s.vector_score,
        s.hybrid_score,
        r.rerank_score
    FROM #Stage2 s
    LEFT JOIN #Rerank r
        ON r.Id = s.Id
    ORDER BY
        r.rerank_score DESC,   -- primary: reranker
        s.hybrid_score DESC;   -- fallback: hybrid if rerank missing
END;
