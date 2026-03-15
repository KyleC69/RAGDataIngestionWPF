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



namespace RAGDataIngestionWPF.Contracts.ViewModels;





public interface INavigationAware
    {

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
    }