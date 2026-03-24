// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagRetrievalService.cs
// Author: Kyle L. Crowder
// Build Num: 133539



namespace DataIngestionLib.Contracts.Services;





public interface IRagRetrievalService
{
    ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(RagSearchQuery query, CancellationToken cancellationToken = default);
}





public readonly record struct RagSearchQuery(string Query, int TopK = 5, RagSearchMode Mode = RagSearchMode.Hybrid);





public readonly record struct RagSearchResult(int Id, string Title, string Summary, IReadOnlyList<string> Keywords, double Score);





public enum RagSearchMode
{
    FullText, Hybrid
}