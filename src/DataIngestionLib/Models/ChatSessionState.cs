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



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
    {

    public int ContextTokenCount { get; init; }

    public AIChatHistory ContextWindow { get; init; } = [];
    public AIChatHistory History { get; init; } = [];
    }