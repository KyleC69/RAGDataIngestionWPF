using System;
using System.Collections.Generic;
using Microsoft.Data.SqlTypes;

namespace DataIngestionLib.History.HistoryModels;

public partial class ChatHistoryTextChunk
{
    public int ChunkRecordId { get; set; }

    public Guid MessageId { get; set; }

    public long ChunkSetId { get; set; }

    public long ChunkOrder { get; set; }

    public long ChunkOffset { get; set; }

    public long ChunkLength { get; set; }

    public string ChunkText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public SqlVector<float>? Embedding { get; set; }
}
