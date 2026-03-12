using System;
using System.Collections.Generic;

namespace DataIngestionLib.ExternalKnowledge.RAGModels;

public partial class Document
{
    public Guid DocId { get; set; }

    public Guid? Uid { get; set; }

    public string Title { get; set; } = null!;

    public string ContentRaw { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Breadcrumb { get; set; }

    public string? DocHtml { get; set; }

    public string? NormalizedMarkdown { get; set; }

    public string? Hash { get; set; }

    public DateTime? LastFetched { get; set; }

    public string? Url { get; set; }
}
