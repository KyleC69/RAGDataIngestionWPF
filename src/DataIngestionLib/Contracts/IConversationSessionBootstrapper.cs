// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationSessionBootstrapper.cs
// Author: Kyle L. Crowder
// Build Num: 140745



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationSessionBootstrapper
{
    ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default);
}





public class ConversationSessionContext(AIAgent Agent, AgentSession Session, string ConversationId)
{
    public AIAgent Agent { get; } = Agent;
    public AgentSession Session { get; } = Session;
    public string ConversationId { get; } = ConversationId;
    public HistoryIdentity Identity { get; set; }
}