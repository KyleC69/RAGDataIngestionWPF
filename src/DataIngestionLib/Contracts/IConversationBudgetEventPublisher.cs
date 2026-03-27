// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationBudgetEventPublisher.cs
// Author: Kyle L. Crowder
// Build Num: 072938



namespace DataIngestionLib.Contracts.Services;





public interface IConversationBudgetEventPublisher
{
    void Publish(ConversationBudgetEvaluation evaluation, int contextTokenCount, Action onSessionBudgetExceeded, Action onTokenBudgetExceeded, Action<int> onMaximumContextWarning);
}