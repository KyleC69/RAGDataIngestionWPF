using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;




public interface IRagContextOrchestrator
{
    ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default);
}