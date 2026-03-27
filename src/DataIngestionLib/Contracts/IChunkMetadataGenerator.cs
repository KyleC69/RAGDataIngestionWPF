// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//

namespace DataIngestionLib.Contracts;

public interface IChunkMetadataGenerator
{
    Task<GeneratedChunkMetadata> GenerateAsync(string chunkContent, CancellationToken cancellationToken = default);
}

public readonly record struct GeneratedChunkMetadata(string? Keywords, string? Summary);
