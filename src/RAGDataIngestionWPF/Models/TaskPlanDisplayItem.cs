using DataIngestionLib.Models;

namespace RAGDataIngestionWPF.Models;

public sealed record TaskPlanDisplayItem(
    Guid PlanId,
    string PlanName,
    string Status,
    string CurrentStep,
    string UpdatedAt,
    string ArtifactSummary)
{
    public static TaskPlanDisplayItem Create(ConversationProgressLog plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        ConversationProgressStep currentStep = plan.Steps.FirstOrDefault(step => step.Id == plan.CurrentStepId)
            ?? plan.Steps.FirstOrDefault(step => step.Status == ConversationProgressStepStatus.InProgress)
            ?? plan.Steps.FirstOrDefault();

        string currentStepText = currentStep is null
            ? "No steps recorded"
            : $"Step {currentStep.Id}: {currentStep.Title} ({currentStep.Status})";

        string artifactSummary = plan.Artifacts.Count == 0
            ? "No artifacts recorded"
            : $"Artifacts: {plan.Artifacts.Count}";

        string updatedAt = plan.UpdatedAtUtc == default
            ? "Updated: unavailable"
            : $"Updated: {plan.UpdatedAtUtc.LocalDateTime:g}";

        return new TaskPlanDisplayItem(
            plan.PlanId,
            plan.PlanName,
            plan.Status.ToString(),
            currentStepText,
            updatedAt,
            artifactSummary);
    }
}