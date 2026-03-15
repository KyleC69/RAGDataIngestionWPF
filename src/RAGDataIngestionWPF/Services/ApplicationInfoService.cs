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
using System.Reflection;

using JetBrains.Annotations;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public sealed class ApplicationInfoService : IApplicationInfoService
    {

    [NotNull]
    public Version GetVersion()
        {
        // Set the app version in RAGDataIngestionWPF > Properties > Package > PackageVersion
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return Version.TryParse(version, out Version parsedVersion)
            ? parsedVersion
            : Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
        }
    }