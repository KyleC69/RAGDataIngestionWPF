// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChunkMetadataGenerator.cs
// Author: Kyle L. Crowder
// Build Num: 072941



namespace DataIngestionLib.Contracts;





public interface IChunkMetadataGenerator
{
    Task<GeneratedChunkMetadata> GenerateAsync(string chunkContent, CancellationToken cancellationToken = default);
}





public readonly record struct GeneratedChunkMetadata(string? Keywords, string? Summary);