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



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class ListDetailsViewModel : ObservableObject, INavigationAware
    {
    private readonly ISampleDataService _sampleDataService;








    public ListDetailsViewModel(ISampleDataService sampleDataService)
        {
        _sampleDataService = sampleDataService;
        }








    public ObservableCollection<SampleOrder> SampleItems { get; } = [];





    [ObservableProperty]
    public partial SampleOrder Selected { get; set; }








    public async void OnNavigatedTo(object parameter)
        {
        SampleItems.Clear();

        IEnumerable<SampleOrder> data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (SampleOrder item in data)
            {
            SampleItems.Add(item);
            }

        Selected = SampleItems.First();
        }








    public void OnNavigatedFrom()
        {
        }
    }