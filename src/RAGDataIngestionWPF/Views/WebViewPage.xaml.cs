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



using Microsoft.Web.WebView2.Core;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class WebViewPage
    {
    private readonly WebViewViewModel _viewModel;








    public WebViewPage(WebViewViewModel viewModel)
        {
        this.InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.Initialize(WebView);
        }








    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
        _viewModel.OnNavigationCompleted(sender, e);
        }
    }