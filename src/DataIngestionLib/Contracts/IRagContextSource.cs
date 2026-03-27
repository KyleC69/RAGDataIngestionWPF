// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextSource.cs
// Author: Kyle L. Crowder
// Build Num: 072940



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
{
    ValueTask<List<ChatMessage>> GetContextMessagesAsync(List<ChatMessage> requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
}