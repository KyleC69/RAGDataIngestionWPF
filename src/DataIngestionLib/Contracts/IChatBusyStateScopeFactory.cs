// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatBusyStateScopeFactory.cs
// Author: Kyle L. Crowder
// Build Num: 140742



namespace DataIngestionLib.Contracts.Services;





public interface IChatBusyStateScopeFactory
{
    IDisposable Enter(Action<bool> busyStateCallback);
}