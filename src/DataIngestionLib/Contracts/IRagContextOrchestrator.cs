// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextOrchestrator.cs
// Author: Kyle L. Crowder
// Build Num: 072940



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextOrchestrator
{
    ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default);
}