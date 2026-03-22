// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Metadata.cs
// Author: Kyle L. Crowder
// Build Num: 140808



using System.ComponentModel.DataAnnotations;




namespace DataIngestionLib.RAGModels;





public sealed class Metadata
{

    public Guid DocId { get; init; }
    public Guid MetaId { get; init; }

    [MaxLength(1024)] public string? Tags { get; init; }
}