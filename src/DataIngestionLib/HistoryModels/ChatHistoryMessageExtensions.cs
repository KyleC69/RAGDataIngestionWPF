using DataIngestionLib.History.HistoryModels;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.HistoryModels;

public static class ChatHistoryMessageExtensions
{
    public static IReadOnlyList<ChatMessage> ToChatMessages(this IEnumerable<ChatHistoryMessage>? messages)
    {
        if (messages is null)
        {
            return [];
        }

        return messages
            .Where(m => m is not null && !string.IsNullOrWhiteSpace(m.Content))
            .OrderBy(m => m.TimestampUtc)
            .ThenBy(m => m.CreatedAt)
            .Select(m => m.ToChatMessage())
            .ToList();
    }

    public static ChatMessage ToChatMessage(this ChatHistoryMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new ChatMessage(ToChatRole(message.Role), message.Content.Trim())
        {
            MessageId = message.MessageId.ToString("D"),
            CreatedAt = message.TimestampUtc
        };
    }

    private static ChatRole ToChatRole(string? role) =>
        role?.Trim().ToLowerInvariant() switch
        {
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            "user" => ChatRole.User,
            _ => ChatRole.User
        };
}