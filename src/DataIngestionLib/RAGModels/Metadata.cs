// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//


using System.ComponentModel.DataAnnotations;


namespace DataIngestionLib.RAGModels;





public sealed class Metadata
    {

    public Guid DocId { get; init; }
    public Guid MetaId { get; init; }

    [MaxLength(1024)]
    public string? Tags { get; init; }
    }