// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ShellWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 140915



using System.Windows.Controls;

using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class ShellWindow : IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
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