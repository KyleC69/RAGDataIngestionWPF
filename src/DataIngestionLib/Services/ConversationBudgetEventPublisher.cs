// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationBudgetEventPublisher.cs
// Author: Kyle L. Crowder
// Build Num: 133601



using DataIngestionLib.Contracts.Services;




namespace DataIngestionLib.Services;





public sealed class ConversationBudgetEventPublisher : IConversationBudgetEventPublisher
{
    public void Publish(ConversationBudgetEvaluation evaluation, int contextTokenCount, Action onSessionBudgetExceeded, Action onTokenBudgetExceeded, Action<int> onMaximumContextWarning)
    {
        ArgumentNullException.ThrowIfNull(onSessionBudgetExceeded);
        ArgumentNullException.ThrowIfNull(onTokenBudgetExceeded);
        ArgumentNullException.ThrowIfNull(onMaximumContextWarning);

        if (evaluation.SessionBudgetExceeded)
        {
            onSessionBudgetExceeded();
            onTokenBudgetExceeded();
            return;
        }

        if (evaluation.MaximumContextWarning)
        {
            onMaximumContextWarning(contextTokenCount);
        }
    }
}