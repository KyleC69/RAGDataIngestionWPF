using Microsoft.Agents.AI;

using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;



namespace DataIngestionLib.Services;

internal sealed class ChatHistoryMemoryProvider : MessageAIContextProvider
{
    private const string SessionStateKey = nameof(ChatHistoryMemoryProvider);
    private const int MaxStoredMessagesPerSession = 40;
    private const int MaxContextMessages = 8;

    private readonly Dictionary<string, List<AIChatMessage>> _historyBySession = [];



    /// <summary>
    /// Provides a collection of chat messages for the specified invoking context.
    /// </summary>
    /// <param name="context">The invoking context containing session and other relevant information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable collection
    /// of <see cref="Microsoft.Extensions.AI.ChatMessage"/> objects, representing the chat messages
    /// associated with the current session.
    /// </returns>
    /// <remarks>
    /// This method retrieves the most recent chat messages from the session's history, limited to a maximum
    /// number of context messages. If no messages are available for the session, an empty collection is returned.
    /// </remarks>
    protected override ValueTask<IEnumerable<AIChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string sessionKey = GetOrCreateSessionKey(context.Session);

        lock (_historyBySession)
        {
            return !_historyBySession.TryGetValue(sessionKey, out List<AIChatMessage>? sessionMessages) || sessionMessages.Count == 0
                ? ValueTask.FromResult<IEnumerable<AIChatMessage>>([])
                : ValueTask.FromResult<IEnumerable<AIChatMessage>>(sessionMessages.TakeLast(MaxContextMessages).ToArray());
        }
    }



    /// <summary>
    /// Stores the AI context, including request and response messages, into the session's chat history.
    /// </summary>
    /// <param name="context">
    /// The invoked context containing session information, request messages, and response messages.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// This method saves the provided request and response messages into the session's history, ensuring
    /// that the total number of stored messages does not exceed the maximum allowed. If the session's
    /// history exceeds the limit, the oldest messages are removed to maintain the size constraint.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(AIContextProvider.InvokedContext context, CancellationToken cancellationToken = default)
    {
        // In a perfect world, I wouldn't want to limit the context messages, I want to model to remember what it was doing, it is so frustrating to have to remind the model of what it just generated.
        //
        //However, this is an in-memory provider, this may cause OOM. Does this double memory usage? Memory to store and the memory used in the model?
        // TODO: This will be changed to sql Vector DB, so we can store more messages and not worry about OOM. For now, we will limit the messages to avoid OOM.
        cancellationToken.ThrowIfCancellationRequested();

        if (context.InvokeException is not null)
        {
            return ValueTask.CompletedTask;
        }

        string sessionKey = GetOrCreateSessionKey(context.Session);
        List<AIChatMessage> messagesToStore = context.RequestMessages
                .Concat(context.ResponseMessages ?? [])
                .ToList();

        if (messagesToStore.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        lock (_historyBySession)
        {
            if (!_historyBySession.TryGetValue(sessionKey, out List<AIChatMessage>? sessionMessages))
            {
                sessionMessages = [];
                _historyBySession[sessionKey] = sessionMessages;
            }

            sessionMessages.AddRange(messagesToStore);

            if (sessionMessages.Count > MaxStoredMessagesPerSession)
            {
                int removeCount = sessionMessages.Count - MaxStoredMessagesPerSession;
                sessionMessages.RemoveRange(0, removeCount);
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string GetOrCreateSessionKey(AgentSession session)
    {
        if (session.StateBag.TryGetValue<string>(SessionStateKey, out string? existingSessionKey) &&
            !string.IsNullOrWhiteSpace(existingSessionKey))
        {
            return existingSessionKey;
        }

        string sessionKey = Guid.NewGuid().ToString("N");
        session.StateBag.SetValue(SessionStateKey, sessionKey);
        return sessionKey;
    }
}
