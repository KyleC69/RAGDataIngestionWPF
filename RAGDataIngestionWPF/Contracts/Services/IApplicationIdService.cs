// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IApplicationIdService.cs
// Author: Kyle L. Crowder
// Build Num: 013430



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IApplicationIdService
{
    Guid GetApplicationId();


    Guid RenewApplicationId();
}