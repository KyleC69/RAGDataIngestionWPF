// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 140743



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistoryProvider
{

    ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default);




    ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);


    ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default);
}