// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RagQueryExpander.cs
// Author: Kyle L. Crowder
// Build Num: 133611



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public sealed class RagQueryExpander : IRagQueryExpander
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
            "the",
            "and",
            "for",
            "with",
            "that",
            "this",
            "from",
            "into",
            "about",
            "what",
            "when",
            "where",
            "which",
            "while",
            "have",
            "does",
            "how",
            "why",
            "can",
            "should"
    };








    public IReadOnlyList<RagSearchQuery> Expand(IReadOnlyList<ChatMessage> requestMessages)
    {
        ArgumentNullException.ThrowIfNull(requestMessages);

        var latestQuery = requestMessages.Where(message => message.Role == ChatRole.User).Select(message => message.Text?.Trim() ?? string.Empty).LastOrDefault(text => !string.IsNullOrWhiteSpace(text)) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(latestQuery))
        {
            return [];
        }

        List<RagSearchQuery> queries = [new(latestQuery)];
        var keywordQuery = string.Join(" ", Tokenize(latestQuery).Take(8));
        if (!string.IsNullOrWhiteSpace(keywordQuery) && !string.Equals(keywordQuery, latestQuery, StringComparison.OrdinalIgnoreCase))
        {
            queries.Add(new RagSearchQuery(keywordQuery));
        }

        return queries;
    }








    internal static IReadOnlyList<string> Tokenize(string text)
    {
        return text.Split([
                        ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '/', '\\', '"', '\''
                ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(term => term.Trim())
                .Where(term => term.Length >= 3 && !StopWords.Contains(term))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}