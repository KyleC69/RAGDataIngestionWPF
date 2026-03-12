using System;
using System.Collections.Generic;

namespace DataIngestionLib.ExternalKnowledge.RAGModels;

public partial class Metadata
{
    public Guid MetaId { get; set; }

    public Guid DocId { get; set; }

    public string? Tags { get; set; }
}
