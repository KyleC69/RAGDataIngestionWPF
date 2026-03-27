// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 073033



using System.Collections.ObjectModel;

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.DocIngestion;
using DataIngestionLib.RAGModels;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly IAppCancellationTokenProvider _appCancellationProvider;
    private readonly ILogger<DataGridViewModel> _logger;
    private readonly DocIngestionPipeline _runner;
    private readonly SqlTableMaint _sqlTableMaint;
    private AsyncRelayCommand _cancelOperationsCommand;
    private AsyncRelayCommand _refreshRemoteRagCommand;
    private LinkedCancellationTokenScope? _pageOperationScope;
    private AsyncRelayCommand _startIngestionCommand;








    public DataGridViewModel()
    {
    }








    public DataGridViewModel(ILogger<DataGridViewModel> logger, DocIngestionPipeline runner, SqlTableMaint sqlTableMaint, IAppCancellationTokenProvider appCancellationProvider)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(runner);
        Guard.IsNotNull(sqlTableMaint);
        Guard.IsNotNull(appCancellationProvider);

        _logger = logger;
        _runner = runner;
        _sqlTableMaint = sqlTableMaint;
        _appCancellationProvider = appCancellationProvider;
    }








    public IAsyncRelayCommand CancelOperationsCommand
    {
        get { return _cancelOperationsCommand ??= new AsyncRelayCommand(CancelOperations); }
    }

    public IAsyncRelayCommand GenerateMetaEmbeddingsCommand
    {
        get { return _refreshRemoteRagCommand ??= new AsyncRelayCommand(RefreshRemoteRagSource); }
    }








    private Task RefreshRemoteRagSource(CancellationToken arg)
    {
        //Refreshes the remote RAG source records in database by checking web for updates in the source documents.
        return Task.CompletedTask;
    }








    public ObservableCollection<RemoteRag> Source { get; } = [];

    public IAsyncRelayCommand StartIngestionCommand
    {
        get { return _startIngestionCommand ??= new AsyncRelayCommand(StartIngestion); }
    }








    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // Create a new linked cancellation scope for this page's operations if the provider is available
        // (It may be null if using the parameterless constructor for design-time scenarios)
        if (_appCancellationProvider != null)
        {
            _pageOperationScope = _appCancellationProvider.CreateLinkedScope();
            _logger?.LogInformation("DataGridPage navigation: created scoped cancellation token.");
        }
    }








    public void OnNavigatedFrom()
    {
        // Cancel and dispose the page-specific operation scope when navigating away
        if (_pageOperationScope != null)
        {
            _logger.LogInformation("DataGridPage navigation: cancelling scoped operations.");
            _pageOperationScope.Cancel();
            _pageOperationScope.Dispose();
            _pageOperationScope = null;
        }
    }








    private async Task CancelOperations()
    {
        _logger?.LogInformation("User initiated operation cancellation.");
        _pageOperationScope?.Cancel();

        // Give the operation a moment to handle the cancellation
        await Task.Delay(100);
    }








    private async Task GenerateMetaEmbeddings()
    {
        try
        {
            Guard.IsNotNull(_sqlTableMaint);

            // Use the page-scoped cancellation token if available, otherwise use CancellationToken.None
            CancellationToken token = _pageOperationScope?.Token ?? CancellationToken.None;
            MetadataUpdateResult result = await _sqlTableMaint.UpdateMetadataAsync(token);
            _logger.LogInformation("Metadata update completed. Updated {UpdatedCount} chunk(s); {FailedCount} failed.", result.UpdatedCount, result.FailedCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Metadata update was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metadata update failed: {Message}", ex.Message);
        }
    }








    private async Task StartIngestion()
    {

        try
        {
            Guard.IsNotNull(_runner);

            // Use the page-scoped cancellation token if available, otherwise use CancellationToken.None
            CancellationToken token = _pageOperationScope?.Token ?? CancellationToken.None;
            await _runner.DoIngestionAsync(token);
            _logger.LogInformation("Document ingestion completed successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Document ingestion was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document ingestion failed: {Message}", ex.Message);
        }





    }
}