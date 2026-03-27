// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 073021



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}