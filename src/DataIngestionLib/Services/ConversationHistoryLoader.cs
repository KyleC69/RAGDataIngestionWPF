// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationHistoryLoader.cs
// Author: Kyle L. Crowder
// Build Num: 140820



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





/// <summary>
///     Provides functionality to load conversation history from a data source.
/// </summary>
/// <remarks>
///     This class is responsible for retrieving and transforming persisted chat messages into a format
///     suitable for use within the application. It utilizes an <see cref="ISQLChatHistoryProvider" /> to
///     fetch the persisted messages and processes them into <see cref="ChatMessage" /> instances.
/// </remarks>
public sealed class ConversationHistoryLoader : IConversationHistoryLoader
{
    private readonly ISQLChatHistoryProvider? _sqlChatHistoryProvider;
    private readonly IAppSettings _appSettings;








    /// <summary>
    ///     Initializes a new instance of the <see cref="ConversationHistoryLoader" /> class.
    /// </summary>
    /// <param name="sqlChatHistoryProvider">
    ///     An optional instance of <see cref="ISQLChatHistoryProvider" /> used to retrieve persisted chat messages.
    ///     If not provided, the loader will operate without a data source.
    /// </param>
    /// <remarks>
    ///     This constructor allows dependency injection of an <see cref="ISQLChatHistoryProvider" /> to enable
    ///     fetching and processing of conversation history. Passing <c>null</c> will result in the loader
    ///     functioning with no external data source.
    /// </remarks>
    public ConversationHistoryLoader( IAppSettings settings,ISQLChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _appSettings = settings;
    }








    /// <summary>
    ///     Asynchronously loads the conversation history for a specified conversation ID.
    /// </summary>
    /// <param name="conversationId">
    ///     The unique identifier of the conversation whose history is to be loaded.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of
    ///     <see cref="ChatMessage" /> objects representing the conversation history.
    /// </returns>
    /// <remarks>
    ///     If the <see cref="ISQLChatHistoryProvider" /> is not initialized or the provided conversation ID is null or
    ///     whitespace,
    ///     an empty list is returned. Messages with empty content are skipped during the mapping process.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="conversationId" /> is null or whitespace.
    /// </exception>
    public async ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(HistoryIdentity identity, CancellationToken cancellationToken = default)
    {
        if (_sqlChatHistoryProvider is null || identity is null)
        {
            return [];
        }
      
      
        var persistedMessages = await _sqlChatHistoryProvider.GetMessagesAsync(identity, cancellationToken).ConfigureAwait(false);

        List<ChatMessage> historyMessages = [];
        foreach (PersistedChatMessage persistedMessage in persistedMessages)
        {
            if (string.IsNullOrWhiteSpace(persistedMessage.Content))
            {
                continue;
            }

            var roleValue = persistedMessage.Role?.Trim() ?? string.Empty;
            ChatRole role = roleValue.Length == 0 ? ChatRole.User : new ChatRole(roleValue);

            historyMessages.Add(new ChatMessage(role, persistedMessage.Content) { CreatedAt = persistedMessage.TimestampUtc, MessageId = persistedMessage.MessageId.ToString("D") });
        }

        return historyMessages;
    }
}