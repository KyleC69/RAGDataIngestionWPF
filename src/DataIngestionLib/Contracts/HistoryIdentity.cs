// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryIdentity.cs
// Author: Kyle L. Crowder
// Build Num: 140742



namespace DataIngestionLib.Services.Contracts;





public record HistoryIdentity
{

    public string AgentId { get; set; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}