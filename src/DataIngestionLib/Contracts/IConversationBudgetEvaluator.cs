using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationBudgetEvaluator
{
    ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget);
}





public readonly record struct ConversationBudgetEvaluation(bool SessionBudgetExceeded, bool MaximumContextWarning);