// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         INavigationAware.cs
//   Author: Kyle L. Crowder



namespace RAGDataIngestionWPF.Contracts.ViewModels;





public interface INavigationAware
{

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
}