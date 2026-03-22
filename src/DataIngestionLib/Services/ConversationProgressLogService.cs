// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationProgressLogService.cs
// Author: Kyle L. Crowder
// Build Num: 140821



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;




namespace DataIngestionLib.Services;





public sealed class ConversationProgressLogService : IConversationProgressLogService
{
    private readonly IConversationProgressLogStore _store;








    public ConversationProgressLogService(IConversationProgressLogStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }








    public async ValueTask AbandonPlanAsync(string conversationId, Guid planId, string? reason = null, CancellationToken cancellationToken = default)
    {
        ConversationProgressLog plan = await GetRequiredPlanAsync(conversationId, planId, cancellationToken).ConfigureAwait(false);
        Dictionary<string, string> artifacts = new(plan.Artifacts, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            artifacts["abandon_reason"] = reason;
        }

        ConversationProgressLog updated = plan with { Status = ConversationProgressStatus.Abandoned, Artifacts = artifacts, UpdatedAtUtc = DateTimeOffset.Now };

        await _store.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog> CompletePlanAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default)
    {
        ConversationProgressLog plan = await GetRequiredPlanAsync(conversationId, planId, cancellationToken).ConfigureAwait(false);
        ConversationProgressLog updated = plan with { Status = ConversationProgressStatus.Completed, UpdatedAtUtc = DateTimeOffset.Now, Steps = plan.Steps.Select(step => step.Status == ConversationProgressStepStatus.NotStarted ? step with { Status = ConversationProgressStepStatus.Completed } : step).ToArray() };

        await _store.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated;
    }








    public async ValueTask<ConversationProgressLog> CreatePlanAsync(string conversationId, string planName, IReadOnlyList<string> stepTitles, CancellationToken cancellationToken = default)
    {
        var normalizedConversationId = conversationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedConversationId))
        {
            throw new ArgumentException("Conversation ID is required.", nameof(conversationId));
        }

        var normalizedPlanName = planName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedPlanName))
        {
            throw new ArgumentException("Plan name is required.", nameof(planName));
        }

        ArgumentNullException.ThrowIfNull(stepTitles);
        if (stepTitles.Count == 0)
        {
            throw new ArgumentException("At least one step is required.", nameof(stepTitles));
        }

        var steps = stepTitles.Select((title, index) => new ConversationProgressStep { Id = index + 1, Title = title?.Trim() ?? string.Empty, Status = index == 0 ? ConversationProgressStepStatus.InProgress : ConversationProgressStepStatus.NotStarted }).ToArray();

        ConversationProgressLog plan = new()
        {
                PlanId = Guid.NewGuid(),
                ConversationId = normalizedConversationId,
                PlanName = normalizedPlanName,
                CurrentStepId = 1,
                Status = ConversationProgressStatus.InProgress,
                Steps = steps,
                UpdatedAtUtc = DateTimeOffset.Now
        };

        await _store.SaveAsync(plan, cancellationToken).ConfigureAwait(false);
        return plan;
    }








    public ValueTask<ConversationProgressLog?> GetPlanAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default)
    {
        return _store.GetAsync(conversationId, planId, cancellationToken);
    }








    public ValueTask<IReadOnlyList<ConversationProgressLog>> ListPlansAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        return _store.ListAsync(conversationId, cancellationToken);
    }








    public async ValueTask<ConversationProgressLog> RecordArtifactAsync(string conversationId, Guid planId, string artifactKey, string artifactValue, CancellationToken cancellationToken = default)
    {
        var normalizedArtifactKey = artifactKey?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedArtifactKey))
        {
            throw new ArgumentException("Artifact key is required.", nameof(artifactKey));
        }

        ConversationProgressLog plan = await GetRequiredPlanAsync(conversationId, planId, cancellationToken).ConfigureAwait(false);
        Dictionary<string, string> artifacts = new(plan.Artifacts, StringComparer.OrdinalIgnoreCase) { [normalizedArtifactKey] = artifactValue ?? string.Empty };

        ConversationProgressLog updated = plan with { Artifacts = artifacts, UpdatedAtUtc = DateTimeOffset.Now };

        await _store.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated;
    }








    public async ValueTask<ConversationProgressLog> SetCurrentStepAsync(string conversationId, Guid planId, int stepId, ConversationProgressStepStatus status, CancellationToken cancellationToken = default)
    {
        if (stepId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stepId));
        }

        ConversationProgressLog plan = await GetRequiredPlanAsync(conversationId, planId, cancellationToken).ConfigureAwait(false);
        if (!plan.Steps.Any(step => step.Id == stepId))
        {
            throw new InvalidOperationException($"Plan '{planId}' does not contain step '{stepId}'.");
        }

        var steps = plan.Steps.Select(step => UpdateStep(step, stepId, status)).ToArray();

        ConversationProgressLog updated = plan with { CurrentStepId = stepId, Steps = steps, UpdatedAtUtc = DateTimeOffset.Now };

        await _store.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated;
    }








    internal async ValueTask<ConversationProgressLog> GetRequiredPlanAsync(string conversationId, Guid planId, CancellationToken cancellationToken)
    {
        ConversationProgressLog? plan = await _store.GetAsync(conversationId, planId, cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            throw new InvalidOperationException($"No plan with ID '{planId}' exists for conversation '{conversationId}'.");
        }

        return plan;
    }








    internal static ConversationProgressStep UpdateStep(ConversationProgressStep step, int targetStepId, ConversationProgressStepStatus status)
    {
        if (step.Id == targetStepId)
        {
            return step with { Status = status };
        }

        if (status == ConversationProgressStepStatus.InProgress && step.Id < targetStepId && step.Status == ConversationProgressStepStatus.InProgress)
        {
            return step with { Status = ConversationProgressStepStatus.Completed };
        }

        return step;
    }
}