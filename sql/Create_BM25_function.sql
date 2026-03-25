CREATE OR ALTER FUNCTION dbo.fn_BM25
(
    @Document NVARCHAR(MAX),
    @Query NVARCHAR(MAX)
)
RETURNS FLOAT
AS
BEGIN
    DECLARE 
        @k1 FLOAT = 1.5,
        @b  FLOAT = 0.75,
        @Score FLOAT = 0.0;

    --------------------------------------------------------------------
    -- Tokenize query terms
    --------------------------------------------------------------------
    ;WITH QueryTerms AS
    (
        SELECT value AS term
        FROM STRING_SPLIT(@Query, ' ')
        WHERE value <> ''
    ),

    --------------------------------------------------------------------
    -- Tokenize document terms
    --------------------------------------------------------------------
    DocTerms AS
    (
        SELECT value AS term
        FROM STRING_SPLIT(@Document, ' ')
        WHERE value <> ''
    ),

    --------------------------------------------------------------------
    -- Term frequency per query term
    --------------------------------------------------------------------
    TF AS
    (
        SELECT 
            q.term,
            COUNT(d.term) AS tf
        FROM QueryTerms q
        LEFT JOIN DocTerms d
            ON d.term = q.term
        GROUP BY q.term
    ),

    --------------------------------------------------------------------
    -- Document length
    --------------------------------------------------------------------
    DocLen AS
    (
        SELECT COUNT(*) AS len
        FROM DocTerms
    )

    --------------------------------------------------------------------
    -- Compute BM25
    --------------------------------------------------------------------
    SELECT @Score = SUM(
        (
            (tf.tf * (@k1 + 1)) /
            (tf.tf + @k1 * (1 - @b + @b * (DocLen.len / 1000.0)))
        )
    )
    FROM TF
    CROSS JOIN DocLen;

    RETURN @Score;
END;
GO