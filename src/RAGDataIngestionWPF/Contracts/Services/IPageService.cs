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



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPageService
    {

    Page GetPage(string key);


    Type GetPageType(string key);
    }