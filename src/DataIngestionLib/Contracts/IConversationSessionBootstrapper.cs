// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



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