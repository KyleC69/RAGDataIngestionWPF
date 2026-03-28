// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationBudgetEvaluator.cs
// Author: Kyle L. Crowder
// Build Num: 073002



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;




namespace DataIngestionLib.Services;





public sealed class ConversationBudgetEvaluator
{
    public ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget)
    {
        var sessionBudgetExceeded = contextTokenCount >= budget.SessionBudget;
        var maximumContextWarning = !sessionBudgetExceeded && contextTokenCount >= budget.MaximumContext;
        return new ConversationBudgetEvaluation(sessionBudgetExceeded, maximumContextWarning);
    }
}