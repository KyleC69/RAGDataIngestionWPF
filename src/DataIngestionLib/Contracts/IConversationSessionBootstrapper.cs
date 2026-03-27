// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationSessionBootstrapper.cs
// Author: Kyle L. Crowder
// Build Num: 072939



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IConversationSessionBootstrapper
{
    ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default);
}





public class ConversationSessionContext(AIAgent agent, AgentSession session, string conversationId)
{
    public AIAgent Agent { get; } = agent;
    public string ConversationId { get; } = conversationId;
    public HistoryIdentity Identity { get; set; } = new();
    public AgentSession Session { get; } = session;
}