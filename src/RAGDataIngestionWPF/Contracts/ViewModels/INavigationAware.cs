// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         INavigationAware.cs
// Author: Kyle L. Crowder
// Build Num: 073026



namespace RAGDataIngestionWPF.Contracts.ViewModels;





public interface INavigationAware
{

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
}