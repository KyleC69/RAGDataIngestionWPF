// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 073024



namespace RAGDataIngestionWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}