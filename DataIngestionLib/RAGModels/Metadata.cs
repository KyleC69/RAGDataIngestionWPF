// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Metadata.cs
// Author: Kyle L. Crowder
// Build Num: 202403



namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public sealed class Metadata
{

    public Guid DocId { get; init; }
    public Guid MetaId { get; init; }

    public string? Tags { get; init; }
}