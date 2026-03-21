using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationHistoryLoader
{
    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(string conversationId, CancellationToken cancellationToken = default);
}