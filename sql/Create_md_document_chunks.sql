USE AIDataRAG;
GO

-- ===========================================================================
-- Table: dbo.md_document_chunks
-- Purpose: Stores individual section chunks derived from md_documents.
--          One chunk = one logical section of a document, bounded by markdown
--          headers (H1–H6) or a fenced code block that is large enough to be
--          useful in retrieval.
--
-- Design notes:
--   - A chunk is either 'text' (prose content of a header-delimited section)
--     or 'code' (a multi-line fenced code block treated as its own unit).
--     Single-line code snippets are discarded during ingestion because they
--     carry no actionable context on their own.
--   - heading_path stores the full breadcrumb (e.g. "IChatClient > Tool
--     calling > Function invocation") so the RAG prompt can include section
--     ancestry without re-fetching the parent document.
--   - VECTOR(1536) matches OpenAI text-embedding-3-small / Ollama nomic-embed-
--     text (adjust dimension as needed).  The column is nullable so chunks
--     can be inserted before embeddings are generated asynchronously.
--   - Full-text index on (content, heading_path, summary) enables BM25 and
--     supplies the keyword column populated by the trigger pattern already
--     used by this database.
--   - chunk_index is a zero-based ordinal for reconstructing reading order.
-- ===========================================================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ---------------------------------------------------------------------------
-- 1. Table definition
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.md_document_chunks', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[md_document_chunks]
    (
        -- Surrogate primary key.
        [chunk_id]       UNIQUEIDENTIFIER NOT NULL
                         CONSTRAINT [DF_md_document_chunks_chunk_id] DEFAULT NEWSEQUENTIALID(),

        -- Parent document.
        [doc_id]         UNIQUEIDENTIFIER NOT NULL,

        -- Zero-based position within the parent document so the original
        -- reading order can always be reconstructed.
        [chunk_index]    INT              NOT NULL,

        -- Breadcrumb assembled from all ancestor headings, e.g.:
        -- "Use the IChatClient interface > Tool calling > Function invocation"
        [heading_path]   NVARCHAR(1024)   NULL,

        -- The immediate section heading for this chunk (without # prefix).
        [heading]        NVARCHAR(512)    NULL,

        -- Markdown heading level of the section delimiter (1–6).
        -- NULL for document preamble chunks that precede the first heading.
        [heading_level]  TINYINT          NULL,

        -- 'text' for prose sections, 'code' for fenced code blocks.
        [chunk_type]     NVARCHAR(16)     NOT NULL
                         CONSTRAINT [DF_md_document_chunks_chunk_type] DEFAULT N'text',

        -- Programming / markup language declared on the fenced code block
        -- (e.g. 'csharp', 'json', 'xml').  NULL for text chunks.
        [language]       NVARCHAR(64)     NULL,

        -- The actual content of this chunk.
        [content]        NVARCHAR(MAX)    NOT NULL,

        -- Token count populated by the ingestion pipeline; used for
        -- budget-aware retrieval.
        [token_count]    INT              NULL,

        -- Dense vector embedding for semantic similarity search.
        -- Dimension matches your embedding model; adjust if needed.
        [embedding]      VECTOR(1536)     NULL,

        -- Space-separated keyword string for BM25 scoring, populated by a
        -- trigger or the ingestion pipeline (mirrors ChatHistoryMessages).
        [keywords]       NVARCHAR(MAX)    NULL,

        -- Short AI-generated summary for result-preview and re-ranking.
        [summary]        NVARCHAR(4000)   NULL,

        [created_at]     DATETIME2(7)     NOT NULL
                         CONSTRAINT [DF_md_document_chunks_created_at] DEFAULT SYSUTCDATETIME(),

        CONSTRAINT [PK_md_document_chunks]
            PRIMARY KEY CLUSTERED ([chunk_id] ASC),

        CONSTRAINT [FK_md_document_chunks_doc_id]
            FOREIGN KEY ([doc_id])
            REFERENCES [dbo].[md_documents] ([doc_id])
            ON DELETE CASCADE
    );
END
GO

-- ---------------------------------------------------------------------------
-- 2. Non-clustered index: fast enumeration of all chunks for a document
--    (used when re-ingesting to delete stale chunks before re-inserting).
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.md_document_chunks')
      AND name = N'IX_md_document_chunks_doc_id_index'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_md_document_chunks_doc_id_index]
        ON [dbo].[md_document_chunks] ([doc_id] ASC, [chunk_index] ASC)
        INCLUDE ([chunk_id], [chunk_type], [heading_path], [heading]);
END
GO

-- ---------------------------------------------------------------------------
-- 3. Non-clustered index: fast vector-candidate recall shortlist by doc_id
--    (used when restricting semantic search to a subset of documents).
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.md_document_chunks')
      AND name = N'IX_md_document_chunks_doc_id_chunk_type'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_md_document_chunks_doc_id_chunk_type]
        ON [dbo].[md_document_chunks] ([doc_id] ASC, [chunk_type] ASC)
        INCLUDE ([chunk_id], [token_count], [heading_path]);
END
GO

-- ---------------------------------------------------------------------------
-- 4. Full-text catalog & index
--    Enables BM25 / FREETEXTTABLE keyword recall mirroring the pattern used
--    by existing stored procedures (sp_Search_Hybrid etc.).
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'MdChunksCatalog'
)
BEGIN
    CREATE FULLTEXT CATALOG [MdChunksCatalog] AS DEFAULT;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.fulltext_indexes
    WHERE object_id = OBJECT_ID(N'dbo.md_document_chunks')
)
BEGIN
    CREATE FULLTEXT INDEX ON [dbo].[md_document_chunks]
    (
        [content]      LANGUAGE 1033,
        [heading_path] LANGUAGE 1033,
        [summary]      LANGUAGE 1033,
        [keywords]     LANGUAGE 1033
    )
    KEY INDEX [PK_md_document_chunks]
    ON [MdChunksCatalog]
    WITH STOPLIST = SYSTEM,
         CHANGE_TRACKING AUTO;
END
GO

-- ---------------------------------------------------------------------------
-- 5. CHECK constraint: valid chunk_type values
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE object_id  = OBJECT_ID(N'CK_md_document_chunks_chunk_type')
      AND parent_object_id = OBJECT_ID(N'dbo.md_document_chunks')
)
BEGIN
    ALTER TABLE [dbo].[md_document_chunks]
        ADD CONSTRAINT [CK_md_document_chunks_chunk_type]
        CHECK ([chunk_type] IN (N'text', N'code'));
END
GO

-- ---------------------------------------------------------------------------
-- 6. CHECK constraint: valid heading_level range
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE object_id  = OBJECT_ID(N'CK_md_document_chunks_heading_level')
      AND parent_object_id = OBJECT_ID(N'dbo.md_document_chunks')
)
BEGIN
    ALTER TABLE [dbo].[md_document_chunks]
        ADD CONSTRAINT [CK_md_document_chunks_heading_level]
        CHECK ([heading_level] IS NULL OR ([heading_level] >= 1 AND [heading_level] <= 6));
END
GO
