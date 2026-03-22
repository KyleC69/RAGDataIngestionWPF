// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationHistoryLoader.cs
// Author: Kyle L. Crowder
// Build Num: 140745



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationHistoryLoader
{
    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(string conversationId, CancellationToken cancellationToken = default);
}