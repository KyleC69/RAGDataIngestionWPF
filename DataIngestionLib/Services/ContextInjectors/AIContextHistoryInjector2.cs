// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AIMemoryProvider.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;





namespace DataIngestionLib.Services.ContextInjectors;


// Looks like this is a partial context injection provider.
// TODO: this needs to be refactored and concerns re-analyzed

/// <summary>
/// </summary>
/// <param name="chatHistoryMemoryProvider"></param>
/// <param name="applicationId"></param>
public sealed class AIContextHistoryInjector2(IChatHistoryMemoryProvider chatHistoryMemoryProvider, IRuntimeContextAccessor accessor) : MessageAIContextProvider
{
    private readonly string _applicationId = accessor.GetCurrent().ApplicationId.ToString();

    private readonly IChatHistoryMemoryProvider _chatHistoryMemoryProvider = chatHistoryMemoryProvider;

    private readonly IRuntimeContextAccessor _runtimeAccessor = accessor;

    //We need get the actual Agent id. Where can we get it from???
    private string DefaultAgentId = "default-agent";








    /// <summary>
    ///     Provides a collection of chat messages for the specified invoking context.
    /// </summary>
    /// <param name="context">The invoking context containing session and other relevant information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable collection
    ///     of <see cref="Microsoft.Extensions.AI.ChatMessage" /> objects, representing the chat messages
    ///     associated with the current session.
    /// </returns>
    /// <remarks>
    ///     This method retrieves the most recent chat messages from the session's history, limited to a maximum
    ///     number of context messages. If no messages are available for the session, an empty collection is returned.
    /// </remarks>
    protected override async ValueTask<IEnumerable<Microsoft.Extensions.AI.ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        ChatHistory requestMessages = ToChatHistory(context.RequestMessages);
        IEnumerable<AIChatMessage> historyMessages = await _chatHistoryMemoryProvider
                .BuildContextMessagesAsync(conversationId, requestMessages, cancellationToken)
                .ConfigureAwait(false);

        return ToFrameworkChatMessages(historyMessages);
    }








    /// <summary>
    ///     Stores the AI context, including request and response messages, into the session's chat history.
    /// </summary>
    /// <param name="context">
    ///     The invoked context containing session information, request messages, and response messages.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method saves the provided request and response messages into the session's history, ensuring
    ///     that the total number of stored messages does not exceed the maximum allowed. If the session's
    ///     history exceeds the limit, the oldest messages are removed to maintain the size constraint.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        if (context.InvokeException is not null)
        {
            return ValueTask.CompletedTask;
        }

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        string sessionId = ChatHistorySessionState.GetOrCreateSessionId(context.Session);
        string agentId = ChatHistorySessionState.GetOrCreateAgentId(context.Session, DefaultAgentId);
        string userId = ChatHistorySessionState.GetOrCreateUserId(context.Session);
        string applicationId = ChatHistorySessionState.GetOrCreateApplicationId(context.Session, _applicationId);

        ChatHistory requestMessages = ToChatHistory(context.RequestMessages);
        ChatHistory responseMessages = ToChatHistory(context.ResponseMessages);

        return _chatHistoryMemoryProvider.StoreMessagesAsync(
                conversationId,
                sessionId,
                agentId,
                userId,
                applicationId,
                requestMessages,
                responseMessages,
                cancellationToken);
    }








    private static ChatHistory ToChatHistory(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        ChatHistory chatHistory = [];
        foreach (Microsoft.Extensions.AI.ChatMessage message in messages)
        {
            chatHistory.Add(new AIChatMessage(message.Role, message.Text));
        }

        return chatHistory;
    }








    private static IEnumerable<Microsoft.Extensions.AI.ChatMessage> ToFrameworkChatMessages(IEnumerable<AIChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (AIChatMessage message in messages)
        {
            yield return new Microsoft.Extensions.AI.ChatMessage(message.Role, message.Text);
        }
    }
}