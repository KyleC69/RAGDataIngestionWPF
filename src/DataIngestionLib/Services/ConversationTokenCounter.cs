// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationTokenCounter.cs
// Author: Kyle L. Crowder
// Build Num: 140823



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationTokenCounter : IConversationTokenCounter
{
    public ConversationTokenSnapshot Calculate(IReadOnlyList<ChatMessage> history, TokenBudget budget, UsageDetails? usageDetails)
    {
        ArgumentNullException.ThrowIfNull(history);

        ConversationTokenSnapshot snapshot = CalculateFromHistory(history, budget);
        if (usageDetails?.AdditionalCounts is null)
        {
            return snapshot;
        }

        var ragTokens = ClampToInt(GetAdditionalCount(usageDetails, snapshot.Rag, "rag", "rag_tokens", "rag_token_count", "rag_context", "retrieval", "retrieval_tokens", "context", "context_tokens"));
        var toolTokens = ClampToInt(GetAdditionalCount(usageDetails, snapshot.Tool, "tool", "tool_tokens", "tool_token_count", "function", "function_tokens"));
        var systemTokens = ClampToInt(GetAdditionalCount(usageDetails, snapshot.System, "system", "system_tokens", "system_token_count", "instruction", "instruction_tokens"));

        var reserved = ragTokens + toolTokens + systemTokens;
        var sessionTokens = Math.Max(0, snapshot.Total - reserved);
        return new ConversationTokenSnapshot(snapshot.Total, sessionTokens, ragTokens, toolTokens, systemTokens);
    }








    internal static ConversationTokenSnapshot CalculateFromHistory(IReadOnlyList<ChatMessage> history, TokenBudget budget)
    {
        var sessionTokens = 0;
        var ragTokens = 0;
        var toolTokens = 0;
        var systemTokens = 0;
        var totalTokens = 0;

        for (var index = history.Count - 1; index >= 0; index--)
        {
            var content = history[index].Text;
            var messageTokenCount = EstimateTokenCount(content);
            if (totalTokens + messageTokenCount > budget.SessionBudget)
            {
                break;
            }

            var role = history[index].Role.Value;
            if (string.Equals(role, AIChatRole.System.Value, StringComparison.OrdinalIgnoreCase))
            {
                systemTokens += messageTokenCount;
            }
            else if (string.Equals(role, AIChatRole.Tool.Value, StringComparison.OrdinalIgnoreCase))
            {
                toolTokens += messageTokenCount;
            }
            else if (string.Equals(role, AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, "rag", StringComparison.OrdinalIgnoreCase))
            {
                ragTokens += messageTokenCount;
            }
            else
            {
                sessionTokens += messageTokenCount;
            }

            totalTokens += messageTokenCount;
        }

        return new ConversationTokenSnapshot(totalTokens, sessionTokens, ragTokens, toolTokens, systemTokens);
    }








    internal static int ClampToInt(long value)
    {
        if (value <= 0)
        {
            return 0;
        }

        return value >= int.MaxValue ? int.MaxValue : (int)value;
    }








    internal static int EstimateTokenCount(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);
    }








    internal static long GetAdditionalCount(UsageDetails usageDetails, int fallback, params string[] keys)
    {
        if (usageDetails.AdditionalCounts is null || usageDetails.AdditionalCounts.Count == 0)
        {
            return fallback;
        }

        foreach (var key in keys)
        {
            foreach (var (countKey, countValue) in usageDetails.AdditionalCounts)
                if (string.Equals(countKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return countValue;
                }
        }

        return fallback;
    }
}