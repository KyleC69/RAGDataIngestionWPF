// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 140743



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    /// <summary>
    ///     Gets the total current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }

    string ConversationId { get; }

    //Tokens used for RAG context, including prompt and response tokens affecting overall context size.
    int RagTokenCount { get; }

    //All token not otherwise accounted for including user
    int SessionTokenCount { get; }

    //Tokens used for system instructions, including prompt and response tokens affecting overall context size.
    int SystemTokenCount { get; }

    //Tokens used for tool calls, including prompt and response tokens affecting overall context size.
    int ToolTokenCount { get; }


    ValueTask AbandonTaskPlanAsync(Guid planId, string? reason = null, CancellationToken token = default);


    event EventHandler<bool> BusyStateChanged;


    ValueTask<ConversationProgressLog> CompleteTaskPlanAsync(Guid planId, CancellationToken token = default);


    ValueTask<ConversationProgressLog?> GetTaskPlanAsync(Guid planId, CancellationToken token = default);


    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token = default);


    ValueTask<IReadOnlyList<ConversationProgressLog>> LoadTaskPlansAsync(CancellationToken token = default);


    ValueTask<ConversationProgressLog> RecordTaskPlanArtifactAsync(Guid planId, string artifactKey, string artifactValue, CancellationToken token = default);


    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    ValueTask<ConversationProgressLog> StartTaskPlanAsync(string planName, IReadOnlyList<string> stepTitles, CancellationToken token = default);


    ValueTask<ConversationProgressLog> UpdateTaskPlanStepAsync(Guid planId, int stepId, ConversationProgressStepStatus status, CancellationToken token = default);
}