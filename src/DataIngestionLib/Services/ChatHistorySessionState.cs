// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistorySessionState.cs
// Author: Kyle L. Crowder
// Build Num: 140812



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





internal static class ChatHistorySessionState
{

    private const string AgentIdStateKey = "AgentId";
    private const string ApplicationIdStateKey = "ApplicationId";
    private const string ConversationIdStateKey = "ConversationId";
    private const string UserIdStateKey = "UserId";

    private static readonly Lock SyncRoot = new();








    public static HistoryIdentity GetHistoryIdentity(AgentSession? contextSession)
    {

        HistoryIdentity identity = new() { AgentId = contextSession.StateBag.GetValue<string>("AgentId") ?? "UnknownAgent", ApplicationId = contextSession.StateBag.GetValue<string>("ApplicationId") ?? "Application-001", ConversationId = contextSession.StateBag.GetValue<string>("ConversationId") ?? "UnknownConversation", UserId = contextSession.StateBag.GetValue<string>("UserId") ?? "UnknownUser" };
        return identity;

    }
}