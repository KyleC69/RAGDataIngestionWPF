// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 133536



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistoryProvider
{

    HistoryIdentity SessionState { get; }


    ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default);


    ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);


    ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(HistoryIdentity identity, CancellationToken cancellationToken = default);
}