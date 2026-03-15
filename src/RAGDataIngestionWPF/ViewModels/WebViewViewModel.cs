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



using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using JetBrains.Annotations;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class WebViewViewModel : ObservableObject
    {

    private readonly ISystemService _systemService;
    private WebView2 _webView;

    // TODO: Set the URI of the page to show by default
    private const string DefaultUrl = "https://docs.microsoft.com/windows/apps/";








    public WebViewViewModel(ISystemService systemService)
        {
        _systemService = systemService;
        Source = DefaultUrl;
        }








    [NotNull]
    public RelayCommand BrowserBackCommand => field ??= new RelayCommand(() => _webView?.GoBack(), () => _webView?.CanGoBack ?? false);





    [NotNull]
    public RelayCommand BrowserForwardCommand => field ??= new RelayCommand(() => _webView?.GoForward(), () => _webView?.CanGoForward ?? false);





    [ObservableProperty]
    public partial Visibility FailedMesageVisibility { get; set; } = Visibility.Collapsed;


    public bool IsLoading
        {
        get;
        set
            {
            _ = this.SetProperty(ref field, value);
            IsLoadingVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        } = true;





    [ObservableProperty]
    public partial Visibility IsLoadingVisibility { get; set; } = Visibility.Visible;





    public bool IsShowingFailedMessage
        {
        get;
        set
            {
            _ = this.SetProperty(ref field, value);
            FailedMesageVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }





    [NotNull]
    public ICommand OpenInBrowserCommand => field ??= new RelayCommand(this.OnOpenInBrowser);





    [NotNull]
    public ICommand RefreshCommand => field ??= new RelayCommand(this.OnRefresh);





    [ObservableProperty]
    public partial string Source { get; set; }








    public void Initialize(WebView2 webView)
        {
        _webView = webView;
        }








    public void OnNavigationCompleted(object sender, [CanBeNull] CoreWebView2NavigationCompletedEventArgs e)
        {
        IsLoading = false;
        if (e != null && !e.IsSuccess)
            {
            // Use `e.WebErrorStatus` to vary the displayed message based on the error reason
            IsShowingFailedMessage = true;
            }

        BrowserBackCommand.NotifyCanExecuteChanged();
        BrowserForwardCommand.NotifyCanExecuteChanged();
        }








    private void OnOpenInBrowser()
        {
        _systemService.OpenInWebBrowser(Source);
        }








    private void OnRefresh()
        {
        IsShowingFailedMessage = false;
        IsLoading = true;
        _webView?.Reload();
        }
    }