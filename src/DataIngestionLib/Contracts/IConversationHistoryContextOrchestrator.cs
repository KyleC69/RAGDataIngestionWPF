// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationHistoryContextOrchestrator.cs
// Author: Kyle L. Crowder
// Build Num: 072939



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationHistoryContextOrchestrator
{
    ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(string conversationId, IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default);
}