USE [AIChatHistory]
GO

-- ===========================================================================
-- Table: dbo.md_documents
-- Purpose: Stores fully normalized markdown documents ingested from the docs
--          folder tree. Each row holds the raw markdown (after YAML is
--          stripped), the normalized version (includes resolved, links
--          flattened), and the YAML front-matter fields extracted for rich
--          metadata filtering.
--
-- Design notes:
--   - file_path is relative to the ingestion root and acts as the natural key
--     so re-ingesting the same file performs an UPSERT (UPDATE + INSERT).
--   - content_hash (SHA-256 hex) lets the pipeline skip re-hashing blocks
--     that are unchanged on subsequent runs.
--   - Full-text indexing is placed on (title, description, content_normalized)
--     to support BM25-style recall alongside vector search on the chunks.
--   - yaml_front_matter stores the raw --- block for audit / replay purposes.
-- ===========================================================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ---------------------------------------------------------------------------
-- 1. Table definition
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.md_documents', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[md_documents]
    (
        -- Surrogate key; NEWSEQUENTIALID keeps the clustered index warm.
        [doc_id]             UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT [DF_md_documents_doc_id] DEFAULT NEWSEQUENTIALID(),

        -- Relative path from ingestion root (e.g. "ai\ichatclient.md").
        -- Drives the UPSERT identity.
        [file_path]          NVARCHAR(512)    NOT NULL,

        -- Bare filename for display / filtering.
        [file_name]          NVARCHAR(260)    NOT NULL,

        -- ---- YAML front-matter fields (nullable – not all docs have them) ----

        -- title: from YAML or derived from the first H1.
        [title]              NVARCHAR(512)    NULL,

        -- description: from YAML ms.date description field.
        [description]        NVARCHAR(2048)   NULL,

        -- author: from YAML author field.
        [author]             NVARCHAR(256)    NULL,

        -- ms_date: parsed from YAML ms.date (e.g. 12/10/2025).
        [ms_date]            DATE             NULL,

        -- ms_topic: from YAML ms.topic (concept-article, how-to, overview…).
        [ms_topic]           NVARCHAR(256)    NULL,

        -- Full raw YAML block (content between the first pair of --- delimiters)
        -- kept verbatim for debugging, replay, and any future custom fields.
        [yaml_front_matter]  NVARCHAR(4000)   NULL,

        -- ---- Content fields ------------------------------------------------

        -- Raw markdown after the YAML front-matter block is removed.
        [content_raw]        NVARCHAR(MAX)    NOT NULL,

        -- Normalized version: DocFX :::code::: includes resolved inline,
        -- [text](url) hyperlinks flattened to plain text, xref tokens
        -- simplified.  This is what the chunker operates on.
        [content_normalized] NVARCHAR(MAX)    NULL,

        -- SHA-256 hex (64 chars) of content_normalized; used for
        -- change-detection on subsequent ingestion runs.
        [content_hash]       NVARCHAR(64)     NOT NULL,

        -- Approximate word count of content_normalized.
        [word_count]         INT              NULL,

        -- ---- Audit fields --------------------------------------------------
        [ingested_at]        DATETIME2(7)     NOT NULL
                             CONSTRAINT [DF_md_documents_ingested_at] DEFAULT SYSUTCDATETIME(),

        -- Set on every update pass so the pipeline can track re-ingestions.
        [updated_at]         DATETIME2(7)     NULL,

        CONSTRAINT [PK_md_documents]
            PRIMARY KEY CLUSTERED ([doc_id] ASC),

        CONSTRAINT [UQ_md_documents_file_path]
            UNIQUE NONCLUSTERED ([file_path] ASC)
    );
END
GO

-- ---------------------------------------------------------------------------
-- 2. Non-clustered index on ms_date for chronological filtering
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.md_documents')
      AND name = N'IX_md_documents_ms_date'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_md_documents_ms_date]
        ON [dbo].[md_documents] ([ms_date] ASC)
        INCLUDE ([doc_id], [title], [file_path]);
END
GO

-- ---------------------------------------------------------------------------
-- 3. Full-text catalog & index
--    Supports BM25-style keyword recall in the hybrid search procedure,
--    mirroring the pattern used by ChatHistoryMessages.
-- ---------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'MdDocsCatalog'
)
BEGIN
    CREATE FULLTEXT CATALOG [MdDocsCatalog] AS DEFAULT;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.fulltext_indexes
    WHERE object_id = OBJECT_ID(N'dbo.md_documents')
)
BEGIN
    CREATE FULLTEXT INDEX ON [dbo].[md_documents]
    (
        [title]              LANGUAGE 1033,
        [description]        LANGUAGE 1033,
        [content_normalized] LANGUAGE 1033
    )
    KEY INDEX [PK_md_documents]
    ON [MdDocsCatalog]
    WITH STOPLIST = SYSTEM,
         CHANGE_TRACKING AUTO;
END
GO
