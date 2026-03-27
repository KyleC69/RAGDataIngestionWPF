// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatBusyStateScopeFactory.cs
// Author: Kyle L. Crowder
// Build Num: 072937



namespace DataIngestionLib.Contracts;





public interface IChatBusyStateScopeFactory
{
    IDisposable Enter(Action<bool> busyStateCallback);
}