namespace DataIngestionLib.Contracts.Services;




public interface IConversationBudgetEventPublisher
{
    void Publish(
            ConversationBudgetEvaluation evaluation,
            int contextTokenCount,
            Action onSessionBudgetExceeded,
            Action onTokenBudgetExceeded,
            Action<int> onMaximumContextWarning);
}