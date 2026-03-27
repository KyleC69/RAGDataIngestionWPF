// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationHistoryLoader.cs
// Author: Kyle L. Crowder
// Build Num: 072939



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationHistoryLoader
{
    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(HistoryIdentity conversationId, CancellationToken cancellationToken = default);
}