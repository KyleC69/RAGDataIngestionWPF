using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationSessionBootstrapper
{
    ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default);
}





public readonly record struct ConversationSessionContext(AIAgent Agent, AgentSession Session, string ConversationId);