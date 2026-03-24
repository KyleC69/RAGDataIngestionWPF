// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;




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