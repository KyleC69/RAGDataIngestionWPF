// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationSessionBootstrapper.cs
// Author: Kyle L. Crowder
// Build Num: 140745



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationSessionBootstrapper
{
    ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default);
}





public readonly record struct ConversationSessionContext(AIAgent Agent, AgentSession Session, string ConversationId);