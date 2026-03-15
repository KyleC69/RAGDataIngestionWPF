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

using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class ShellWindow : IShellWindow
    {
    public ShellWindow(ShellViewModel viewModel)
        {
        this.InitializeComponent();
        DataContext = viewModel;
        }








    public Frame GetNavigationFrame()
        {
        return ShellFrame;
        }








    public void ShowWindow()
        {
        this.Show();
        }








    public void CloseWindow()
        {
        this.Close();
        }
    }