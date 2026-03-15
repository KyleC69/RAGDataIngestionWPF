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



using System.Diagnostics;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public sealed class SystemService : ISystemService
    {

    public void OpenInWebBrowser(string url)
        {
        // For more info see https://github.com/dotnet/corefx/issues/10361
        ProcessStartInfo psi = new()
            {
            FileName = url,
            UseShellExecute = true
            };
        _ = Process.Start(psi);
        }
    }