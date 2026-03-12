// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextSource.cs
// Author: Kyle L. Crowder
// Build Num: 013504



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
{
    ValueTask<AIChatHistory> GetContextMessagesAsync(AIChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
}