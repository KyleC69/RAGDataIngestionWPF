using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DataIngestionLib.Providers;

public sealed class ConversationCacheContextSource : IRagContextSource
{
    private const string ConversationIdStateKey = "ConversationId";
    private const string ChatHistoryConversationIdStateKey = "ChatHistoryConversationId";

    private readonly IAppSettings _appSettings;
    private readonly IConversationContextCacheStore _cacheStore;
    private readonly IContextCitationFormatter _citationFormatter;
    private readonly ILogger<ConversationCacheContextSource> _logger;

    public ConversationCacheContextSource(
            IConversationContextCacheStore cacheStore,
            IAppSettings appSettings,
            IContextCitationFormatter citationFormatter,
            ILogger<ConversationCacheContextSource> logger)
    {
        ArgumentNullException.ThrowIfNull(cacheStore);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(citationFormatter);
        ArgumentNullException.ThrowIfNull(logger);

        _cacheStore = cacheStore;
        _appSettings = appSettings;
        _citationFormatter = citationFormatter;
        _logger = logger;
    }

    public async ValueTask<List<ChatMessage>> GetContextMessagesAsync(List<ChatMessage> requestMessages, AgentSession? session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        string query = requestMessages
                .Where(message => message.Role == ChatRole.User)
                .Select(message => message.Text?.Trim() ?? string.Empty)
                .LastOrDefault(text => !string.IsNullOrWhiteSpace(text))
                ?? string.Empty;
        string conversationId = ResolveConversationId(session);
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(conversationId))
        {
            return [];
        }

        try
        {
            IReadOnlyList<ConversationContextCacheEntry> entries = await _cacheStore
                    .SearchAsync(conversationId, query, 3, cancellationToken)
                    .ConfigureAwait(false);
            if (entries.Count == 0)
            {
                return [];
            }

            int maxCharacters = Math.Max(300, _appSettings.MetaBudget * 4);
            string body = _citationFormatter.FormatSection(
                "Relevant cached context",
                [
                    .. entries.Select(entry => new ContextCitation
                    {
                        Title = entry.Role?.Trim() ?? AIChatRole.RAGContext.Value,
                        SourceKind = "conversation-cache",
                        Locator = entry.EntryId.ToString("D"),
                        TimestampUtc = entry.CreatedAtUtc,
                        Content = entry.Text
                    })
                ],
                maxCharacters);

            if (string.IsNullOrWhiteSpace(body))
            {
                return [];
            }

            return
            [
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), body)
            ];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to retrieve cached context for conversation {ConversationId}.", conversationId);
            return [];
        }
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