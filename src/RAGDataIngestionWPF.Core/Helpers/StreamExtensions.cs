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



namespace RAGDataIngestionWPF.Core.Helpers;





public static class StreamExtensions
    {
    public static string ToBase64String(this Stream stream)
        {
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
        }
    }