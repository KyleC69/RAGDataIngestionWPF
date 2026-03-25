// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 140906



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.DocIngestion;
using DataIngestionLib.RAGModels;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly ILogger<DataGridViewModel> _logger;
    private readonly LearningHtmlRunner _runner;
    private AsyncRelayCommand _startIngestionCommand;








    public DataGridViewModel()
    {
    }








    public DataGridViewModel(ILogger<DataGridViewModel> logger, LearningHtmlRunner runner)
    {
        _logger = logger;
        _runner = runner;
    }








    public ObservableCollection<RemoteRag> Source { get; } = [];

    public IAsyncRelayCommand StartIngestionCommand
    {
        get { return _startIngestionCommand ??= new AsyncRelayCommand(StartIngestion); }
    }








    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

    }








    public void OnNavigatedFrom()
    {
    }








    private async Task StartIngestion()
    {

        try
        {
            await _runner.IngestRemoteKnowledgeSource();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);


        }





    }
}