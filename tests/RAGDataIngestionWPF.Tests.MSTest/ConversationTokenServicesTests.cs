using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ConversationTokenServicesTests
{
    private static TokenBudget MakeBudget(
        int sessionBudget = 100_000,
        int maximumContext = 80_000,
        int systemBudget = 5_000,
        int ragBudget = 10_000,
        int toolBudget = 5_000,
        int metaBudget = 1_000)
        => new()
        {
            SessionBudget = sessionBudget,
            MaximumContext = maximumContext,
            SystemBudget = systemBudget,
            RAGBudget = ragBudget,
            ToolBudget = toolBudget,
            MetaBudget = metaBudget,
            BudgetTotal = sessionBudget + systemBudget + ragBudget + toolBudget + metaBudget
        };

    [TestMethod]
    public void CounterCalculatesRoleBucketsFromHistory()
    {
        IConversationTokenCounter counter = new ConversationTokenCounter();
        TokenBudget budget = MakeBudget();
        IReadOnlyList<ChatMessage> history =
        [
            new(ChatRole.User, "abcd"),
            new(new ChatRole(AIChatRole.System.Value), "abcdefgh"),
            new(new ChatRole(AIChatRole.Tool.Value), "abcd"),
            new(new ChatRole(AIChatRole.RAGContext.Value), "abcdefgh")
        ];

        ConversationTokenSnapshot snapshot = counter.Calculate(history, budget, null);

        Assert.AreEqual(6, snapshot.Total);
        Assert.AreEqual(1, snapshot.Session);
        Assert.AreEqual(2, snapshot.System);
        Assert.AreEqual(1, snapshot.Tool);
        Assert.AreEqual(2, snapshot.Rag);
    }

    [TestMethod]
    public void CounterUsesAdditionalCountsWhenProvided()
    {
        IConversationTokenCounter counter = new ConversationTokenCounter();
        TokenBudget budget = MakeBudget();
        IReadOnlyList<ChatMessage> history =
        [
            new(ChatRole.User, "abcdefgh"),
            new(new ChatRole(AIChatRole.RAGContext.Value), "abcdefgh"),
            new(new ChatRole(AIChatRole.Tool.Value), "abcdefgh")
        ];

        UsageDetails usage = new()
        {
            AdditionalCounts = new AdditionalPropertiesDictionary<long>
            {
                ["rag_tokens"] = 7,
                ["tool_tokens"] = 3,
                ["system_tokens"] = 2
            }
        };

        ConversationTokenSnapshot snapshot = counter.Calculate(history, budget, usage);

        Assert.AreEqual(6, snapshot.Total);
        Assert.AreEqual(7, snapshot.Rag);
        Assert.AreEqual(3, snapshot.Tool);
        Assert.AreEqual(2, snapshot.System);
        Assert.AreEqual(0, snapshot.Session);
    }

    [TestMethod]
    public void BudgetEvaluatorSignalsSessionBudgetExceededBeforeMaximumContext()
    {
        IConversationBudgetEvaluator evaluator = new ConversationBudgetEvaluator();
        TokenBudget budget = MakeBudget(sessionBudget: 5, maximumContext: 4);

        ConversationBudgetEvaluation evaluation = evaluator.Evaluate(5, budget);

        Assert.IsTrue(evaluation.SessionBudgetExceeded);
        Assert.IsFalse(evaluation.MaximumContextWarning);
    }

    [TestMethod]
    public void BudgetEvaluatorSignalsMaximumContextWarningWhenBelowSessionBudget()
    {
        IConversationBudgetEvaluator evaluator = new ConversationBudgetEvaluator();
        TokenBudget budget = MakeBudget(sessionBudget: 10, maximumContext: 4);

        ConversationBudgetEvaluation evaluation = evaluator.Evaluate(4, budget);

        Assert.IsFalse(evaluation.SessionBudgetExceeded);
        Assert.IsTrue(evaluation.MaximumContextWarning);
    }
}