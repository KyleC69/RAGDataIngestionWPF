// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ContextCitationFormatter.cs
// Author: Kyle L. Crowder
// Build Num: 140816



using System.Globalization;
using System.Text;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;




namespace DataIngestionLib.Services;





public sealed class ContextCitationFormatter : IContextCitationFormatter
{
    public string FormatSection(string heading, IReadOnlyList<ContextCitation> citations, int maxCharacters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(citations);

        List<string> blocks = [];
        var currentCharacters = 0;

        foreach (ContextCitation citation in citations)
        {
            var content = citation.Content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var block = FormatBlock(blocks.Count + 1, citation with { Content = content });
            if (currentCharacters > 0 && currentCharacters + Environment.NewLine.Length * 2 + block.Length > maxCharacters)
            {
                break;
            }

            blocks.Add(block);
            currentCharacters += block.Length + (blocks.Count > 1 ? Environment.NewLine.Length * 2 : 0);
        }

        if (blocks.Count == 0)
        {
            return string.Empty;
        }

        return heading.Trim() + ":" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, blocks);
    }








    internal static string FormatBlock(int index, ContextCitation citation)
    {
        StringBuilder builder = new();
        _ = builder.Append('[').Append(index).Append("] ").Append(citation.Title.Trim());

        List<string> metadata = [];
        var sourceKind = citation.SourceKind?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(sourceKind))
        {
            metadata.Add("source=" + sourceKind);
        }

        var locator = citation.Locator?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(locator))
        {
            metadata.Add("locator=" + locator);
        }

        if (citation.TimestampUtc.HasValue)
        {
            metadata.Add("timestamp=" + citation.TimestampUtc.Value.UtcDateTime.ToString("u", CultureInfo.InvariantCulture));
        }

        if (metadata.Count > 0)
        {
            _ = builder.Append(" (").Append(string.Join("; ", metadata)).Append(')');
        }

        _ = builder.AppendLine();
        _ = builder.Append(citation.Content);
        return builder.ToString();
    }
}