// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 073039



using System.Windows;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class DataGridPage
{
    public DataGridPage(DataGridViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

#if !SQL
        DisableSQL();
#endif
    }








    /// <summary>
    ///     Disables SQL-related functionality in the UI by disabling specific controls
    ///     such as buttons and enabling a message indicating that SQL is disabled.
    /// </summary>
    /// <remarks>
    ///     This method is typically used in scenarios where SQL functionality is not available
    ///     or supported, ensuring that users are informed and restricted from interacting
    ///     with SQL-dependent features.
    /// </remarks>
    private void DisableSQL()
    {
        RagDataGrid.IsEnabled = false;
        StartIngestionBtn.IsEnabled = false;
        GenerateBtn.IsEnabled = false;
        CancelBtn.IsEnabled = false;
        noSqlTb.Visibility = Visibility.Visible;

    }
}