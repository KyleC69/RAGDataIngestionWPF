using System.Reflection;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DataIngestionLib.Providers;

public sealed class ConversationContextCacheRecorder : MessageAIContextProvider
{
    private const string ConversationIdStateKey = "ConversationId";
    private const string ChatHistoryConversationIdStateKey = "ChatHistoryConversationId";

    private readonly IConversationContextCacheStore _cacheStore;
    private readonly ILogger<ConversationContextCacheRecorder> _logger;

    public ConversationContextCacheRecorder(
            IConversationContextCacheStore cacheStore,
            ILogger<ConversationContextCacheRecorder> logger)
    {
        ArgumentNullException.ThrowIfNull(cacheStore);
        ArgumentNullException.ThrowIfNull(logger);

        _cacheStore = cacheStore;
        _logger = logger;
    }

    protected override ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult<IEnumerable<ChatMessage>>([]);
    }

    protected override async ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        string conversationId = ResolveConversationId(context.Session);
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        try
        {
            List<ChatMessage> cacheableMessages =
            [
                .. FilterCacheableMessages(context.RequestMessages),
                .. FilterCacheableMessages(GetContextMessages(context, "ResponseMessages")),
                .. FilterCacheableMessages(GetContextMessages(context, "ResultMessages"))
            ];

            if (cacheableMessages.Count == 0)
            {
                return;
            }

            await _cacheStore.AppendAsync(conversationId, cacheableMessages, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to record conversation context cache items for conversation {ConversationId}.", conversationId);
        }
    }

    private static IEnumerable<ChatMessage> FilterCacheableMessages(IEnumerable<ChatMessage>? messages)
    {
        return messages?.Where(static message =>
                !string.IsNullOrWhiteSpace(message.Text)
                && IsCacheableRole(message.Role))
            ?? [];
    }

    private static IReadOnlyList<ChatMessage> GetContextMessages(object context, string propertyName)
    {
        PropertyInfo? property = context.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(context) is IEnumerable<ChatMessage> messages)
        {
            return messages.Where(static message => !string.IsNullOrWhiteSpace(message.Text)).ToArray();
        }

        return [];
    }

    private static bool IsCacheableRole(ChatRole role)
    {
        string value = role.Value?.Trim() ?? string.Empty;
        return value.Equals(AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase)
               || value.Equals(AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase)
               || value.Equals(AIChatRole.Tool.Value, StringComparison.OrdinalIgnoreCase)
               || value.EndsWith("_context", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveConversationId(AgentSession? session)
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