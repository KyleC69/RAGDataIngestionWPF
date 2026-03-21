using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ConversationEventServicesTests
{
    [TestMethod]
    public void BusyStateScopeEmitsTrueThenFalse()
    {
        IChatBusyStateScopeFactory factory = new ChatBusyStateScopeFactory();
        List<bool> states = [];

        using (factory.Enter(states.Add))
        {
        }

        CollectionAssert.AreEqual(new[] { true, false }, states);
    }

    [TestMethod]
    public void BusyStateScopeDisposeIsIdempotent()
    {
        IChatBusyStateScopeFactory factory = new ChatBusyStateScopeFactory();
        List<bool> states = [];

        IDisposable scope = factory.Enter(states.Add);
        scope.Dispose();
        scope.Dispose();

        CollectionAssert.AreEqual(new[] { true, false }, states);
    }

    [TestMethod]
    public void BudgetPublisherRaisesSessionAndTokenEventsWhenExceeded()
    {
        IConversationBudgetEventPublisher publisher = new ConversationBudgetEventPublisher();
        bool sessionExceeded = false;
        bool tokenExceeded = false;
        bool maximumWarning = false;

        publisher.Publish(
            new ConversationBudgetEvaluation(true, false),
            10,
            () => sessionExceeded = true,
            () => tokenExceeded = true,
            _ => maximumWarning = true);

        Assert.IsTrue(sessionExceeded);
        Assert.IsTrue(tokenExceeded);
        Assert.IsFalse(maximumWarning);
    }

    [TestMethod]
    public void BudgetPublisherRaisesMaximumContextWarningWhenApplicable()
    {
        IConversationBudgetEventPublisher publisher = new ConversationBudgetEventPublisher();
        int? warningCount = null;

        publisher.Publish(
            new ConversationBudgetEvaluation(false, true),
            7,
            () => Assert.Fail("Session budget event should not fire."),
            () => Assert.Fail("Token budget event should not fire."),
            count => warningCount = count);

        Assert.AreEqual(7, warningCount);
    }
}