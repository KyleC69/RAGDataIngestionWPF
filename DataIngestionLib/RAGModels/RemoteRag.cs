using System;
using System.Collections.Generic;
using Microsoft.Data.SqlTypes;

namespace DataIngestionLib.ExternalKnowledge.RAGModels;

public partial class RemoteRag
{
    public int Id { get; set; }

    public Guid DocumentId { get; set; }

    public string OgUrl { get; set; } = null!;

    public DateTime MsDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Keywords { get; set; }

    public string? Summary { get; set; }

    public SqlVector<float>? Embedding { get; set; }

    public int? TokenCount { get; set; }

    public int? Version { get; set; }

    public double? Score { get; set; }
}
