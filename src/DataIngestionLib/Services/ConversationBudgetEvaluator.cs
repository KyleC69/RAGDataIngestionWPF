using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Services;





public sealed class ConversationBudgetEvaluator : IConversationBudgetEvaluator
{
    public ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget)
    {
        bool sessionBudgetExceeded = contextTokenCount >= budget.SessionBudget;
        bool maximumContextWarning = !sessionBudgetExceeded && contextTokenCount >= budget.MaximumContext;
        return new ConversationBudgetEvaluation(sessionBudgetExceeded, maximumContextWarning);
    }
}