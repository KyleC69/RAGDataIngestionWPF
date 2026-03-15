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



using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class LogInWindow : ILogInWindow
    {
    public LogInWindow(LogInViewModel viewModel)
        {
        this.InitializeComponent();
        DataContext = viewModel;
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