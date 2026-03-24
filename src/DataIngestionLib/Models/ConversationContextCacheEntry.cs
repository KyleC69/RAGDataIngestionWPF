// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationContextCacheEntry.cs
// Author: Kyle L. Crowder
// Build Num: 133549



namespace DataIngestionLib.Models;





public sealed record ConversationContextCacheEntry
{
    public DateTimeOffset CreatedAtUtc { get; init; }

    public Guid EntryId { get; init; }

    public string[] Keywords { get; init; } = [];

    public string Role { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;
}