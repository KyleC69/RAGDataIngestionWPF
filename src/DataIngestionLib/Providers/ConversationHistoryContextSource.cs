using DataIngestionLib.Contracts.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DataIngestionLib.Providers;

public sealed class ConversationHistoryContextSource : IRagContextSource
{
    private const string ConversationIdStateKey = "ConversationId";
    private const string ChatHistoryConversationIdStateKey = "ChatHistoryConversationId";

    private readonly ILogger<ConversationHistoryContextSource> _logger;
    private readonly IConversationHistoryContextOrchestrator _orchestrator;

    public ConversationHistoryContextSource(
            IConversationHistoryContextOrchestrator orchestrator,
            ILogger<ConversationHistoryContextSource> logger)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(logger);

        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async ValueTask<List<ChatMessage>> GetContextMessagesAsync(List<ChatMessage> requestMessages, AgentSession? session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        string conversationId = ResolveConversationId(session);
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return [];
        }

        try
        {
            IReadOnlyList<ChatMessage> messages = await _orchestrator
                    .BuildContextMessagesAsync(conversationId, requestMessages, cancellationToken)
                    .ConfigureAwait(false);

            return [.. messages];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to build conversation history context for conversation {ConversationId}.", conversationId);
            return [];
        }
    }

    internal static string ResolveConversationId(AgentSession? session)
    {
        if (session is null)
        {
            return string.Empty;
        }

        if (session.StateBag.TryGetValue(ConversationIdStateKey, out string? conversationId)
            && !string.IsNullOrWhiteSpace(conversationId))
        {
            return conversationId;
        }

        if (session.StateBag.TryGetValue(ChatHistoryConversationIdStateKey, out string? chatHistoryConversationId)
            && !string.IsNullOrWhiteSpace(chatHistoryConversationId))
        {
            return chatHistoryConversationId;
        }

        return string.Empty;
    }
}