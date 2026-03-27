// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Num: 073024



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}