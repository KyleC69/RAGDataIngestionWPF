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

namespace DataIngestionLib.Contracts;

public interface IAppSettings
    {

    string OllamaHost { get; }
    int OllamaPort { get; }
    string ChatModel { get; }
    string EmbeddingModel { get; }
    string LearnBaseUrl { get; }
    string LogDirectory { get; }
    }