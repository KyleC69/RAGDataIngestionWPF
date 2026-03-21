namespace DataIngestionLib.Models;

public sealed record ConversationProgressLog
{
    public Dictionary<string, string> Artifacts { get; init; } = [];

    public string ConversationId { get; init; } = string.Empty;

    public int CurrentStepId { get; init; }

    public string PlanName { get; init; } = string.Empty;

    public Guid PlanId { get; init; }

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
    InProgress,
    Completed,
    Abandoned
}

public enum ConversationProgressStepStatus
{
    NotStarted,
    InProgress,
    Completed,
    Skipped
}