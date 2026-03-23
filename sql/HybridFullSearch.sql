






create PROCEDURE [dbo].[sp_Search_Hybrid]
    @Query NVARCHAR(4000),
    @Embedding VECTOR(1536),
    @TopN INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    --------------------------------------------------------------------
    -- Stage 1: FREETEXTTABLE recall (broad, natural-language recall)
    --------------------------------------------------------------------
    ;WITH ft AS (
        SELECT TOP (@TopN)
            r.Id,
            r.Content,
            r.Summary,
            r.Keywords,
            r.Embedding,
            ft.RANK AS ft_rank
        FROM FREETEXTTABLE(
                dbo.ChatHistoryMessages,
                (Content, Summary, Keywords),
                @Query
             ) AS ft
        INNER JOIN dbo.ChatHistoryMessages r
            ON r.Id = ft.[KEY]
        ORDER BY ft.RANK DESC
    ),

    --------------------------------------------------------------------
    -- Stage 2A: Vector similarity (semantic meaning)
    --------------------------------------------------------------------
    vec AS (
        SELECT
            f.*,
           1 - VECTOR_DISTANCE('COSINE',f.Embedding, @Embedding) AS vector_score
        FROM ft f
    ),

    --------------------------------------------------------------------
    -- Stage 2B: BM25 lexical scoring (your SQL implementation)
    --------------------------------------------------------------------
    bm AS (
        SELECT
            v.*,
            dbo.fn_BM25(v.Content, @Query) AS bm25_score
        FROM vec v
    ),

    --------------------------------------------------------------------
    -- Stage 2C: Hybrid scoring
    --------------------------------------------------------------------
    ranked AS (
        SELECT
            *,
            -- Weighted hybrid score
            (0.40 * bm25_score) +
            (0.60 * vector_score) AS hybrid_score
        FROM bm
    )

    --------------------------------------------------------------------
    -- Final output for Stage 3 (reranker)
    --------------------------------------------------------------------
    SELECT TOP (@TopN)
        Id,
        Content,
        Summary,
        Keywords,
        ft_rank,
        bm25_score,
        vector_score,
        hybrid_score
    FROM ranked
    ORDER BY hybrid_score DESC;
END;

