// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         TaskPlanDisplayItem.cs
// Author: Kyle L. Crowder
// Build Num: 140859



using DataIngestionLib.Models;




namespace RAGDataIngestionWPF.Models;





public sealed record TaskPlanDisplayItem(Guid PlanId, string PlanName, string Status, string CurrentStep, string UpdatedAt, string ArtifactSummary)
{
    public static TaskPlanDisplayItem Create(ConversationProgressLog plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        ConversationProgressStep currentStep = plan.Steps.FirstOrDefault(step => step.Id == plan.CurrentStepId) ?? plan.Steps.FirstOrDefault(step => step.Status == ConversationProgressStepStatus.InProgress) ?? plan.Steps.FirstOrDefault();

        var currentStepText = currentStep is null ? "No steps recorded" : $"Step {currentStep.Id}: {currentStep.Title} ({currentStep.Status})";

        var artifactSummary = plan.Artifacts.Count == 0 ? "No artifacts recorded" : $"Artifacts: {plan.Artifacts.Count}";

        var updatedAt = plan.UpdatedAtUtc == default ? "Updated: unavailable" : $"Updated: {plan.UpdatedAtUtc.LocalDateTime:g}";

        return new TaskPlanDisplayItem(plan.PlanId, plan.PlanName, plan.Status.ToString(), currentStepText, updatedAt, artifactSummary);
    }
}