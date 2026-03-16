using System;
using System.Collections.Generic;
using Microsoft.Data.SqlTypes;

namespace DataIngestionLib.History.HistoryModels;

public partial class ChatHistoryMessage
{
    public Guid MessageId { get; set; }

    public string ConversationId { get; set; } = null!;

    public string SessionId { get; set; } = null!;

    public string AgentId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string ApplicationId { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }

    public string? Metadata { get; set; }

    public bool? Enabled { get; set; }

    public SqlVector<float>? Embedding { get; set; }

    public string? Summary { get; set; }
}
