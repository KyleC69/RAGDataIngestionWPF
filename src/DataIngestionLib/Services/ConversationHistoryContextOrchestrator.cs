using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Services;

public sealed class ConversationHistoryContextOrchestrator : IConversationHistoryContextOrchestrator
{
    private readonly IAppSettings _appSettings;
    private readonly IContextCitationFormatter _citationFormatter;
    private readonly IConversationHistoryLoader _historyLoader;

    public ConversationHistoryContextOrchestrator(IConversationHistoryLoader historyLoader, IContextCitationFormatter citationFormatter, IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(historyLoader);
        ArgumentNullException.ThrowIfNull(citationFormatter);
        ArgumentNullException.ThrowIfNull(appSettings);

        _historyLoader = historyLoader;
        _citationFormatter = citationFormatter;
        _appSettings = appSettings;
    }

    public async ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            IReadOnlyList<ChatMessage> requestMessages,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return [];
        }

        string query = requestMessages
                .Select(message => message.Text?.Trim() ?? string.Empty)
                .LastOrDefault(text => !string.IsNullOrWhiteSpace(text))
                ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        IReadOnlyList<ChatMessage> historyMessages = await _historyLoader
                .LoadConversationHistoryAsync(conversationId, cancellationToken)
                .ConfigureAwait(false);
        if (historyMessages.Count == 0)
        {
            return [];
        }

        HashSet<string> requestTexts = new(StringComparer.OrdinalIgnoreCase);
        foreach (ChatMessage requestMessage in requestMessages)
        {
            string requestText = requestMessage.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                _ = requestTexts.Add(requestText);
            }
        }

        string[] terms = Tokenize(query);
        List<(ChatMessage Message, double Score)> scoredMessages = [];

        for (int index = 0; index < historyMessages.Count; index++)
        {
            ChatMessage historyMessage = historyMessages[index];
            string content = historyMessage.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content) || requestTexts.Contains(content))
            {
                continue;
            }

            double score = Score(content, query, terms);
            if (score <= 0)
            {
                continue;
            }

            score += (double)(index + 1) / Math.Max(1, historyMessages.Count + 1);
            scoredMessages.Add((historyMessage, score));
        }

        if (scoredMessages.Count == 0)
        {
            return [];
        }

        int maxCharacters = Math.Max(300, _appSettings.MetaBudget * 4);
        string body = _citationFormatter.FormatSection(
            "Relevant conversation history",
            [
                .. scoredMessages
                    .OrderByDescending(entry => entry.Score)
                    .ThenByDescending(entry => entry.Message.CreatedAt)
                    .Take(3)
                    .Select(entry => new ContextCitation
                    {
                        Title = entry.Message.Role.Value?.Trim() ?? "unknown",
                        SourceKind = "conversation-history",
                        Locator = entry.Message.MessageId,
                        TimestampUtc = entry.Message.CreatedAt,
                        Content = entry.Message.Text ?? string.Empty
                    })
            ],
            maxCharacters);

        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        return
        [
            new ChatMessage(new ChatRole(AIChatRole.AIContext.Value), body)
        ];
    }

    internal static double Score(string content, string query, IReadOnlyList<string> terms)
    {
        double score = 0;
        if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        foreach (string term in terms)
        {
            if (content.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }
        }

        return score;
    }

    internal static string[] Tokenize(string text)
    {
        return text
                .Split([
                    ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '/', '\\', '"', '\''
                ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(term => term.Length >= 3)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}