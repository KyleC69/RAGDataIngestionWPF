// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IShellWindow.cs
// Author: Kyle L. Crowder
// Build Num: 140855



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Views;





public interface IShellWindow
{

    void CloseWindow();


    Frame GetNavigationFrame();


    void ShowWindow();
}