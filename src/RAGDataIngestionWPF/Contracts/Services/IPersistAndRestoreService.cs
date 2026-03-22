// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Num: 140854



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}