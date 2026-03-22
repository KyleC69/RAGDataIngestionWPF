// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationProgressLog.cs
// Author: Kyle L. Crowder
// Build Num: 140757



namespace DataIngestionLib.Models;





public sealed record ConversationProgressLog
{
    public Dictionary<string, string> Artifacts { get; init; } = [];

    public string ConversationId { get; init; } = string.Empty;

    public int CurrentStepId { get; init; }

    public Guid PlanId { get; init; }

    public string PlanName { get; init; } = string.Empty;

    public ConversationProgressStatus Status { get; init; } = ConversationProgressStatus.InProgress;

    public IReadOnlyList<ConversationProgressStep> Steps { get; init; } = [];

    public DateTimeOffset UpdatedAtUtc { get; init; }
}





public sealed record ConversationProgressStep
{
    public int Id { get; init; }

    public ConversationProgressStepStatus Status { get; init; } = ConversationProgressStepStatus.NotStarted;

    public string Title { get; init; } = string.Empty;
}





public enum ConversationProgressStatus
{
    InProgress, Completed, Abandoned
}





public enum ConversationProgressStepStatus
{
    NotStarted, InProgress, Completed, Skipped
}