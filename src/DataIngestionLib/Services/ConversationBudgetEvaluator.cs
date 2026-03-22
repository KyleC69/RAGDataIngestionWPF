// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationBudgetEvaluator.cs
// Author: Kyle L. Crowder
// Build Num: 140817



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Services;





public sealed class ConversationBudgetEvaluator : IConversationBudgetEvaluator
{
    public ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget)
    {
        var sessionBudgetExceeded = contextTokenCount >= budget.SessionBudget;
        var maximumContextWarning = !sessionBudgetExceeded && contextTokenCount >= budget.MaximumContext;
        return new ConversationBudgetEvaluation(sessionBudgetExceeded, maximumContextWarning);
    }
}