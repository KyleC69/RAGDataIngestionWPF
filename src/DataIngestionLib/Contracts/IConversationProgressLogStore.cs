// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationProgressLogStore.cs
// Author: Kyle L. Crowder
// Build Num: 133538



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationProgressLogStore
{
    ValueTask DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog?> GetAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default);


    ValueTask<IReadOnlyList<ConversationProgressLog>> ListAsync(string conversationId, CancellationToken cancellationToken = default);


    ValueTask SaveAsync(ConversationProgressLog progressLog, CancellationToken cancellationToken = default);
}