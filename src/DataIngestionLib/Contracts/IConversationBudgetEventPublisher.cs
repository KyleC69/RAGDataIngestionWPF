// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationBudgetEventPublisher.cs
// Author: Kyle L. Crowder
// Build Num: 133536



namespace DataIngestionLib.Contracts.Services;





public interface IConversationBudgetEventPublisher
{
    void Publish(ConversationBudgetEvaluation evaluation, int contextTokenCount, Action onSessionBudgetExceeded, Action onTokenBudgetExceeded, Action<int> onMaximumContextWarning);
}