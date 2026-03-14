// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Metadata.cs
// Author: Kyle L. Crowder
// Build Num: 175055



namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public class Metadata
{

    public Guid DocId { get; set; }
    public Guid MetaId { get; set; }

    public string? Tags { get; set; }
}