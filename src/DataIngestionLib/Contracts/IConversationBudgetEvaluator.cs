// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IConversationBudgetEvaluator.cs
// Author: Kyle L. Crowder
// Build Num: 133536



namespace DataIngestionLib.Contracts.Services;





public interface IConversationBudgetEvaluator
{
    ConversationBudgetEvaluation Evaluate(int contextTokenCount, TokenBudget budget);
}





public readonly record struct ConversationBudgetEvaluation(bool SessionBudgetExceeded, bool MaximumContextWarning);