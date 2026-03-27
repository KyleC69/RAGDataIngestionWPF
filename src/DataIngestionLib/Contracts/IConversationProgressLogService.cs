// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationProgressLogService.cs
// Author: Kyle L. Crowder
// Build Num: 072939



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationProgressLogService
{
    ValueTask AbandonPlanAsync(string conversationId, Guid planId, string? reason = null, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog> CompletePlanAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog> CreatePlanAsync(string conversationId, string planName, IReadOnlyList<string> stepTitles, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog?> GetPlanAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default);


    ValueTask<IReadOnlyList<ConversationProgressLog>> ListPlansAsync(string conversationId, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog> RecordArtifactAsync(string conversationId, Guid planId, string artifactKey, string artifactValue, CancellationToken cancellationToken = default);


    ValueTask<ConversationProgressLog> SetCurrentStepAsync(string conversationId, Guid planId, int stepId, ConversationProgressStepStatus status, CancellationToken cancellationToken = default);
}