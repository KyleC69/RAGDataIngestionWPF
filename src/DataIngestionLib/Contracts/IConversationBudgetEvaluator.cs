// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationBudgetEvaluator.cs
// Author: Kyle L. Crowder
// Build Num: 140744



using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationBudgetEvaluator
{
    ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget);
}





public readonly record struct ConversationBudgetEvaluation(bool SessionBudgetExceeded, bool MaximumContextWarning);