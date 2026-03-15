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



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class ListDetailsPage
    {
    public ListDetailsPage(ListDetailsViewModel viewModel)
        {
        this.InitializeComponent();
        DataContext = viewModel;
        }
    }