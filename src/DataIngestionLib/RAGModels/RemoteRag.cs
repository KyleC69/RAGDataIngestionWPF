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

using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.RAGModels;





public sealed class RemoteRag
    {

    public string Description { get; init; } = null!;

    public Guid DocumentId { get; init; }

    public SqlVector<float>? Embedding { get; init; }
    public int Id { get; init; }

    public string? Keywords { get; init; }

    public DateTime MsDate { get; init; }

    public string OgUrl { get; init; } = null!;

    public double? Score { get; init; }

    [MaxLength(4000)]
    public string? Summary { get; init; }

    public string Title { get; init; } = null!;

    public int? TokenCount { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public int? Version { get; init; }
    }