using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class LocalRagContextSource : IRagContextSource
{
    private readonly ILogger<LocalRagContextSource> _logger;
    private readonly IRagContextOrchestrator _ragContextOrchestrator;





    public LocalRagContextSource(IRagContextOrchestrator ragContextOrchestrator, ILogger<LocalRagContextSource> logger)
    {
        ArgumentNullException.ThrowIfNull(ragContextOrchestrator);
        ArgumentNullException.ThrowIfNull(logger);

        _ragContextOrchestrator = ragContextOrchestrator;
        _logger = logger;
    }





    public async ValueTask<List<ChatMessage>> GetContextMessagesAsync(List<ChatMessage> requestMessages, AgentSession? session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        try
        {
            IReadOnlyList<ChatMessage> contextMessages = await _ragContextOrchestrator
                    .BuildContextMessagesAsync(requestMessages, cancellationToken)
                    .ConfigureAwait(false);
            return contextMessages.ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to build local RAG context.");
            return [];
        }
    }
}