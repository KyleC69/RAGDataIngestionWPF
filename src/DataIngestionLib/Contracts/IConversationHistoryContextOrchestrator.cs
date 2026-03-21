using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts.Services;

public interface IConversationHistoryContextOrchestrator
{
    ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            IReadOnlyList<ChatMessage> requestMessages,
            CancellationToken cancellationToken = default);
}