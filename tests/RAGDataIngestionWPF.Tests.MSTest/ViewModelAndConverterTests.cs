// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ViewModelAndConverterTests.cs
// Author: Kyle L. Crowder
// Build Num: 073108



using System.Globalization;

using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.Logging;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Converters;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.Properties;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ViewModelAndConverterTests
{

    [TestMethod]
    public void AgentLoggerWhitespaceMessageReturnsFailure()
    {
        AgentLogger logger = new();

        var result = logger.LogMessage("  ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Message cannot be null or whitespace.", result.Error);
    }








    [TestMethod]
    public void DataGridViewModelOnNavigatedToClearsSource()
    {
        // Create mock dependencies for the view model
        var mockLogger = new Mock<ILogger<DataGridViewModel>>();
        var mockCancellationProvider = new Mock<IAppCancellationTokenProvider>();
        var mockLinkedScope = new Mock<LinkedCancellationTokenScope>();

        // Setup the mock cancellation provider to return a linked scope
        mockCancellationProvider.Setup(x => x.CreateLinkedScope()).Returns(mockLinkedScope.Object);

        mockLinkedScope.Setup(x => x.Token).Returns(CancellationToken.None);

        // For this test, we only care about OnNavigatedTo clearing the source,
        // so we can use a test that doesn't require the full pipeline
        // Create a minimal test instance without full constructor dependencies
        DataGridViewModel viewModel = new DataGridViewModel();
        if (viewModel.Source == null)
        {
            throw new InvalidOperationException("Source collection should not be null");
        }

        viewModel.Source.Add(new DataIngestionLib.RAGModels.RemoteRag { Title = "t", Description = "d", OgUrl = "u" });
        Assert.AreEqual(1, viewModel.Source.Count, "Should have one item before OnNavigatedTo");

        // Manually test just the clearing behavior for the parameterless constructor
        viewModel.OnNavigatedTo(null);
        Assert.AreEqual(0, viewModel.Source.Count, "Source should be cleared after OnNavigatedTo");
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertBackParsesEnum()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.ConvertBack(true, typeof(AppTheme), nameof(AppTheme.Default), CultureInfo.InvariantCulture);

        Assert.AreEqual(AppTheme.Default, (AppTheme)result);
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertMatchesExpectedEnumValue()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.Convert(AppTheme.Dark, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(true, result);
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertReturnsFalseForMismatchedValue()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.Convert(AppTheme.Light, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(false, result);
    }








    [TestMethod]
    public void LogInViewModelLoginCommandReflectsBusyState()
    {
        Mock<IIdentityService> identity = new Mock<IIdentityService>();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Success);
        LogInViewModel viewModel = new LogInViewModel(identity.Object) { IsBusy = true };

        Assert.IsFalse(viewModel.LoginCommand.CanExecute(null));
    }








    [TestMethod]
    public void LogInViewModelLoginSetsStatusMessageForUnauthorized()
    {
        Mock<IIdentityService> identity = new Mock<IIdentityService>();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Unauthorized);
        LogInViewModel viewModel = new LogInViewModel(identity.Object);

        viewModel.LoginCommand.Execute(null);

        var completed = SpinWait.SpinUntil(() => !viewModel.IsBusy, TimeSpan.FromSeconds(2));

        Assert.IsTrue(completed);
        Assert.AreEqual(Resources.StatusUnauthorized, viewModel.StatusMessage);
    }








    [TestMethod]
    public void WebViewViewModelStateAndCommandsWorkWithoutWebView()
    {
        Mock<ISystemService> systemService = new Mock<ISystemService>();
        WebViewViewModel viewModel = new WebViewViewModel(systemService.Object) { Source = "https://contoso.test" };

        Assert.AreEqual("https://contoso.test", viewModel.Source);

        viewModel.IsLoading = false;
        viewModel.IsShowingFailedMessage = true;

        Assert.AreEqual(System.Windows.Visibility.Collapsed, viewModel.IsLoadingVisibility);
        Assert.AreEqual(System.Windows.Visibility.Visible, viewModel.FailedMesageVisibility);

        viewModel.OpenInBrowserCommand.Execute(null);
        systemService.Verify(service => service.OpenInWebBrowser(viewModel.Source), Times.Once);

        viewModel.RefreshCommand.Execute(null);
        Assert.IsTrue(viewModel.IsLoading);
        Assert.IsFalse(viewModel.IsShowingFailedMessage);

        viewModel.OnNavigationCompleted(this, null);
        Assert.IsFalse(viewModel.IsLoading);
    }
}