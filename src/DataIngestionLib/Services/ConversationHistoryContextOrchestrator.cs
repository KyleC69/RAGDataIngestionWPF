// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationHistoryContextOrchestrator.cs
// Author: Kyle L. Crowder
// Build Num: 073003



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationHistoryContextOrchestrator
{
    private readonly IAppSettings _appSettings;
    private readonly ContextCitationFormatter _citationFormatter;
    private readonly IConversationHistoryLoader _historyLoader;








    public ConversationHistoryContextOrchestrator(IConversationHistoryLoader historyLoader, ContextCitationFormatter citationFormatter, IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(historyLoader);
        ArgumentNullException.ThrowIfNull(citationFormatter);
        ArgumentNullException.ThrowIfNull(appSettings);

        _historyLoader = historyLoader;
        _citationFormatter = citationFormatter;
        _appSettings = appSettings;
    }








    /// <summary>
    ///     Builds a collection of context messages for a given conversation based on the provided request messages.
    /// </summary>
    /// <param name="conversationId">
    ///     The unique identifier of the conversation for which context messages are to be built.
    /// </param>
    /// <param name="requestMessages">
    ///     A collection of <see cref="ChatMessage" /> objects representing the request messages.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of
    ///     <see cref="ChatMessage" /> objects
    ///     representing the context messages.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="requestMessages" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via the <paramref name="cancellationToken" />.
    /// </exception>
    public async ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(string conversationId, IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);
/*
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return [];
        }

        var query = requestMessages.Select(message => message.Text?.Trim() ?? string.Empty).LastOrDefault(text => !string.IsNullOrWhiteSpace(text)) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var historyMessages = await _historyLoader.LoadConversationHistoryAsync(conversationId, cancellationToken).ConfigureAwait(false);
       if (historyMessages.Count == 0)
        {
            return [];
        }

        HashSet<string> requestTexts = new(StringComparer.OrdinalIgnoreCase);
        foreach (ChatMessage requestMessage in requestMessages)
        {
            var requestText = requestMessage.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                _ = requestTexts.Add(requestText);
            }
        }

        var terms = Tokenize(query);
        List<(ChatMessage Message, double Score)> scoredMessages = [];

        for (var index = 0; index < historyMessages.Count; index++)
        {
            ChatMessage historyMessage = historyMessages[index];
            var content = historyMessage.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content) || requestTexts.Contains(content))
            {
                continue;
            }

            var score = Score(content, query, terms);
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

        var maxCharacters = Math.Max(300, _appSettings.MetaBudget * 4);
        var body = _citationFormatter.FormatSection("Relevant conversation history", [
                .. scoredMessages.OrderByDescending(entry => entry.Score)
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
        ], maxCharacters);

        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        return
        [
                new ChatMessage(new ChatRole(AIChatRole.AIContext.Value), body)
        ];*/
        return [];
    }








    internal static double Score(string content, string query, IReadOnlyList<string> terms)
    {
        double score = 0;
        if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        foreach (var term in terms)
            if (content.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }

        return score;
    }








    internal static string[] Tokenize(string text)
    {
        return text.Split([
                        ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '/', '\\', '"', '\''
                ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(term => term.Length >= 3)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}