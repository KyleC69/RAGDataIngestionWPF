// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationAgentRunner.cs
// Author: Kyle L. Crowder
// Build Num: 073002



using DataIngestionLib.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationAgentRunner : IConversationAgentRunner
{
    public async ValueTask<ConversationAgentRunResult> RunAsync(AIAgent agent, string content, AgentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(session);

        AgentResponse response = await agent.RunAsync(content, session, null, cancellationToken).ConfigureAwait(false);
        return new ConversationAgentRunResult(response.Text?.Trim() ?? string.Empty, response.Usage);
    }
}