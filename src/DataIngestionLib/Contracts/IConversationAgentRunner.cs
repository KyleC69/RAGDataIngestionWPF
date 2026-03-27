// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationAgentRunner.cs
// Author: Kyle L. Crowder
// Build Num: 072938



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts;





public readonly record struct ConversationAgentRunResult(string Text, UsageDetails? UsageDetails);





public interface IConversationAgentRunner
{
    ValueTask<ConversationAgentRunResult> RunAsync(AIAgent agent, string content, AgentSession session, CancellationToken cancellationToken);
}