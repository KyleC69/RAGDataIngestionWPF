// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         SettingsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 175120



using System.Windows.Controls;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}