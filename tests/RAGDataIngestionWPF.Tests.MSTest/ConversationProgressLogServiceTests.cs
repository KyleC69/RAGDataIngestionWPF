using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ConversationProgressLogServiceTests
{
    [TestMethod]
    public async Task CreatePlanAsyncInitializesFirstStepInProgress()
    {
        Mock<IConversationProgressLogStore> store = new();
        store.Setup(x => x.SaveAsync(It.IsAny<ConversationProgressLog>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
        IConversationProgressLogService service = new ConversationProgressLogService(store.Object);

        ConversationProgressLog plan = await service.CreatePlanAsync("conversation-1", "rewrite", ["step 1", "step 2"], CancellationToken.None);

        Assert.AreEqual("rewrite", plan.PlanName);
        Assert.AreEqual(1, plan.CurrentStepId);
        Assert.AreEqual(ConversationProgressStepStatus.InProgress, plan.Steps[0].Status);
        Assert.AreEqual(ConversationProgressStepStatus.NotStarted, plan.Steps[1].Status);
    }

    [TestMethod]
    public async Task SetCurrentStepAsyncCompletesPriorInProgressStep()
    {
        Guid planId = Guid.NewGuid();
        Mock<IConversationProgressLogStore> store = new();
        store.Setup(x => x.GetAsync("conversation-1", planId, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite",
                CurrentStepId = 1,
                Steps =
                [
                    new ConversationProgressStep { Id = 1, Title = "step 1", Status = ConversationProgressStepStatus.InProgress },
                    new ConversationProgressStep { Id = 2, Title = "step 2", Status = ConversationProgressStepStatus.NotStarted }
                ]
        });
        store.Setup(x => x.SaveAsync(It.IsAny<ConversationProgressLog>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
        IConversationProgressLogService service = new ConversationProgressLogService(store.Object);

        ConversationProgressLog updated = await service.SetCurrentStepAsync("conversation-1", planId, 2, ConversationProgressStepStatus.InProgress, CancellationToken.None);

        Assert.AreEqual(2, updated.CurrentStepId);
        Assert.AreEqual(ConversationProgressStepStatus.Completed, updated.Steps[0].Status);
        Assert.AreEqual(ConversationProgressStepStatus.InProgress, updated.Steps[1].Status);
    }

    [TestMethod]
    public async Task RecordArtifactAsyncUpsertsArtifact()
    {
        Guid planId = Guid.NewGuid();
        Mock<IConversationProgressLogStore> store = new();
        store.Setup(x => x.GetAsync("conversation-1", planId, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite",
                Artifacts = []
        });
        store.Setup(x => x.SaveAsync(It.IsAny<ConversationProgressLog>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
        IConversationProgressLogService service = new ConversationProgressLogService(store.Object);

        ConversationProgressLog updated = await service.RecordArtifactAsync("conversation-1", planId, "step1", "result", CancellationToken.None);

        Assert.AreEqual("result", updated.Artifacts["step1"]);
    }

    [TestMethod]
    public async Task CompletePlanAsyncMarksPlanCompleted()
    {
        Guid planId = Guid.NewGuid();
        Mock<IConversationProgressLogStore> store = new();
        store.Setup(x => x.GetAsync("conversation-1", planId, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite",
                Status = ConversationProgressStatus.InProgress,
                Steps =
                [
                    new ConversationProgressStep { Id = 1, Title = "step 1", Status = ConversationProgressStepStatus.Completed },
                    new ConversationProgressStep { Id = 2, Title = "step 2", Status = ConversationProgressStepStatus.NotStarted }
                ]
        });
        store.Setup(x => x.SaveAsync(It.IsAny<ConversationProgressLog>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
        IConversationProgressLogService service = new ConversationProgressLogService(store.Object);

        ConversationProgressLog updated = await service.CompletePlanAsync("conversation-1", planId, CancellationToken.None);

        Assert.AreEqual(ConversationProgressStatus.Completed, updated.Status);
        Assert.IsTrue(updated.Steps.All(step => step.Status == ConversationProgressStepStatus.Completed));
    }

    [TestMethod]
    public async Task AbandonPlanAsyncMarksPlanAbandonedAndStoresReason()
    {
        Guid planId = Guid.NewGuid();
        Mock<IConversationProgressLogStore> store = new();
        store.Setup(x => x.GetAsync("conversation-1", planId, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite",
                Status = ConversationProgressStatus.InProgress,
                Artifacts = []
        });

        ConversationProgressLog saved = null;
        store.Setup(x => x.SaveAsync(It.IsAny<ConversationProgressLog>(), It.IsAny<CancellationToken>()))
            .Callback<ConversationProgressLog, CancellationToken>((plan, _) => saved = plan)
            .Returns(ValueTask.CompletedTask);

        IConversationProgressLogService service = new ConversationProgressLogService(store.Object);

        await service.AbandonPlanAsync("conversation-1", planId, "cancelled", CancellationToken.None);

        Assert.IsNotNull(saved);
        Assert.AreEqual(ConversationProgressStatus.Abandoned, saved.Status);
        Assert.AreEqual("cancelled", saved.Artifacts["abandon_reason"]);
    }
}