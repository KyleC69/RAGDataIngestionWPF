// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextSource.cs
// Author: Kyle L. Crowder
// Build Num: 202355



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
    {
    ValueTask<AIChatHistory> GetContextMessagesAsync(AIChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
    }