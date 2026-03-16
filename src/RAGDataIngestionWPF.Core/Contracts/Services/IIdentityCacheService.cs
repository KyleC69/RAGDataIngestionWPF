// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 182436



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}