// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRuntimeContextAccessor.cs
// Author: Kyle L. Crowder
// Build Num: 013504



namespace DataIngestionLib.Contracts.Services;





public sealed record RuntimeContext(
        Guid ApplicationId,
        string? UserPrincipalName,
        string? DisplayName);





public interface IRuntimeContextAccessor
{
    RuntimeContext GetCurrent();
}