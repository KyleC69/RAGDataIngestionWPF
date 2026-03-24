// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationHistoryContextOrchestrator.cs
// Author: Kyle L. Crowder
// Build Num: 133537



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationHistoryContextOrchestrator
{
    ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(string conversationId, IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default);
}