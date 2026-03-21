using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationHistoryLoader : IConversationHistoryLoader
{
    private readonly ISQLChatHistoryProvider? _sqlChatHistoryProvider;





    public ConversationHistoryLoader(ISQLChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
    }





    public async ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        if (_sqlChatHistoryProvider is null || string.IsNullOrWhiteSpace(conversationId))
        {
            return [];
        }

        IReadOnlyList<PersistedChatMessage> persistedMessages = await _sqlChatHistoryProvider
                .GetMessagesAsync(conversationId, null, cancellationToken)
                .ConfigureAwait(false);

        List<ChatMessage> historyMessages = [];
        foreach (PersistedChatMessage persistedMessage in persistedMessages)
        {
            if (string.IsNullOrWhiteSpace(persistedMessage.Content))
            {
                continue;
            }

            string roleValue = persistedMessage.Role?.Trim() ?? string.Empty;
            ChatRole role = roleValue.Length == 0 ? ChatRole.User : new ChatRole(roleValue);

            historyMessages.Add(new ChatMessage(role, persistedMessage.Content)
            {
                    CreatedAt = persistedMessage.TimestampUtc,
                    MessageId = persistedMessage.MessageId.ToString("D")
            });
        }

        return historyMessages;
    }
}