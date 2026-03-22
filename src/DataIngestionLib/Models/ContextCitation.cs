// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ContextCitation.cs
// Author: Kyle L. Crowder
// Build Num: 140757



namespace DataIngestionLib.Models;





public sealed record ContextCitation
{
    public string Content { get; init; } = string.Empty;

    public string? Locator { get; init; }

    public string SourceKind { get; init; } = string.Empty;

    public DateTimeOffset? TimestampUtc { get; init; }

    public string Title { get; init; } = string.Empty;
}