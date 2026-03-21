using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts.Services;

public interface IConversationContextCacheStore
{
    ValueTask AppendAsync(string conversationId, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<ConversationContextCacheEntry>> SearchAsync(
            string conversationId,
            string query,
            int maxResults,
            CancellationToken cancellationToken = default);

    ValueTask ResetAsync(string conversationId, CancellationToken cancellationToken = default);
}