using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts;

public readonly record struct ConversationAgentRunResult(string Text, UsageDetails? UsageDetails);

public interface IConversationAgentRunner
{
    ValueTask<ConversationAgentRunResult> RunAsync(AIAgent agent, string content, AgentSession session, CancellationToken cancellationToken);
}