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



namespace RAGDataIngestionWPF.Models;





public class AppSettings
    {
    public const string CONFIGURATION_SECTION_NAME = "AppConfig";

    public string AppPropertiesFileName { get; init; }

    public string ChatSessionFileName { get; init; }

    public string ConfigurationsFolder { get; init; }

    public string PrivacyStatement { get; init; }
    public string UserFileName { get; init; }
    }