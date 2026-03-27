// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         ISampleDataService.cs
// Author: Kyle L. Crowder
// Build Num: 073021



using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetGridDataAsync();


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}