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





public interface INavigationService
    {

    bool CanGoBack { get; }


    void CleanNavigation();


    void GoBack();


    void Initialize(Frame shellFrame);


    event EventHandler<string> Navigated;


    bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);


    void UnsubscribeNavigation();
    }