// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         PersistedChatMessage.cs
// Author: Kyle L. Crowder
// Build Num: 072952



using System.Text.Json;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Models;





public sealed record PersistedChatMessage
{

    public string AgentId { get; init; } = string.Empty;

    public string ApplicationId { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public string ConversationId { get; init; } = string.Empty;
    public Guid MessageId { get; init; }

    public JsonDocument? Metadata { get; init; }

    public string Role { get; init; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; init; }

    public string UserId { get; init; } = string.Empty;
}





public static class ChatMessageExt
{

    private static ChatRole ParseRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "system" => ChatRole.System,
                _ => ChatRole.User
        };
    }








    public static ChatMessage ToChatMessage(this PersistedChatMessage persistedChatMessage)
    {
        if (persistedChatMessage == null) throw new ArgumentNullException(nameof(persistedChatMessage));

        return new ChatMessage
        {
                AuthorName = null,
                CreatedAt = persistedChatMessage.TimestampUtc,
                MessageId = persistedChatMessage.MessageId.ToString("D"),
                RawRepresentation = null,
                AdditionalProperties = null,
                Role = ParseRole(persistedChatMessage.Role),
                Contents = null
        };
    }
}