// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Document.cs
// Author: Kyle L. Crowder
// Build Num: 202403



namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public sealed class Document
{

    public string? Breadcrumb { get; init; }

    public string ContentRaw { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public string? DocHtml { get; init; }
    public Guid DocId { get; init; }

    public string? Hash { get; init; }

    public DateTime? LastFetched { get; init; }

    public string? NormalizedMarkdown { get; init; }

    public string Title { get; init; } = null!;

    public Guid? Uid { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public string? Url { get; init; }
}