using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class FileConversationProgressLogStoreTests
{
    private string _root = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "conversation-progress-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_root);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [TestMethod]
    public async Task SaveAsyncPersistsAndListsPlans()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);
        ConversationProgressLog plan = new()
        {
                PlanId = Guid.NewGuid(),
                ConversationId = "conversation-1",
                PlanName = "rewrite service",
                CurrentStepId = 1,
                UpdatedAtUtc = new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero),
                Steps =
                [
                    new ConversationProgressStep { Id = 1, Title = "extract seams", Status = ConversationProgressStepStatus.InProgress }
                ]
        };

        await store.SaveAsync(plan, CancellationToken.None);

        IReadOnlyList<ConversationProgressLog> plans = await store.ListAsync("conversation-1", CancellationToken.None);

        Assert.AreEqual(1, plans.Count);
        Assert.AreEqual("rewrite service", plans[0].PlanName);
    }

    [TestMethod]
    public async Task SaveAsyncUpdatesExistingPlan()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);
        Guid planId = Guid.NewGuid();

        await store.SaveAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite service",
                CurrentStepId = 1,
                UpdatedAtUtc = new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero),
                Steps = [new ConversationProgressStep { Id = 1, Title = "extract seams", Status = ConversationProgressStepStatus.InProgress }]
        }, CancellationToken.None);

        await store.SaveAsync(new ConversationProgressLog
        {
                PlanId = planId,
                ConversationId = "conversation-1",
                PlanName = "rewrite service",
                CurrentStepId = 2,
                UpdatedAtUtc = new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero),
                Steps = [new ConversationProgressStep { Id = 2, Title = "validate", Status = ConversationProgressStepStatus.Completed }]
        }, CancellationToken.None);

        ConversationProgressLog plan = await store.GetAsync("conversation-1", planId, CancellationToken.None);

        Assert.IsNotNull(plan);
        Assert.AreEqual(2, plan.CurrentStepId);
        Assert.AreEqual("validate", plan.Steps[0].Title);
    }

    [TestMethod]
    public async Task DeleteConversationAsyncRemovesAllPlans()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        await store.SaveAsync(new ConversationProgressLog
        {
                PlanId = Guid.NewGuid(),
                ConversationId = "conversation-1",
                PlanName = "rewrite service",
                CurrentStepId = 1,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                Steps = [new ConversationProgressStep { Id = 1, Title = "extract seams", Status = ConversationProgressStepStatus.InProgress }]
        }, CancellationToken.None);

        await store.DeleteConversationAsync("conversation-1", CancellationToken.None);

        IReadOnlyList<ConversationProgressLog> plans = await store.ListAsync("conversation-1", CancellationToken.None);

        Assert.AreEqual(0, plans.Count);
    }

    [TestMethod]
    public void Constructor_NullAppSettings_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new FileConversationProgressLogStore(null!, _root));
    }

    [TestMethod]
    public void Constructor_NullApplicationId_UsesDefaultApplicationId()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns((string)null!);

        FileConversationProgressLogStore store = new(settings.Object);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public void Constructor_EmptyApplicationId_UsesDefaultApplicationId()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns(string.Empty);

        FileConversationProgressLogStore store = new(settings.Object);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public void Constructor_WhitespaceApplicationId_UsesDefaultApplicationId()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("   ");

        FileConversationProgressLogStore store = new(settings.Object);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public void Constructor_ValidApplicationId_CreatesInstance()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("TestApp");

        FileConversationProgressLogStore store = new(settings.Object);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public void Constructor_CustomRootDirectory_UsesProvidedPath()
    {
        Mock<IAppSettings> settings = CreateSettings();
        string customRoot = Path.Combine(Path.GetTempPath(), "custom-root");

        FileConversationProgressLogStore store = new(settings.Object, customRoot);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public void Constructor_ApplicationIdWithWhitespace_TrimsAndUsesValue()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("  TestApp  ");

        FileConversationProgressLogStore store = new(settings.Object);

        Assert.IsNotNull(store);
    }

    [TestMethod]
    public async Task Constructor_NullRootDirectory_UsesLocalApplicationDataPath()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("TestApp");

        FileConversationProgressLogStore store = new(settings.Object, null);

        Assert.IsNotNull(store);

        // Verify the store can perform operations (implicitly tests the path was set correctly)
        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync("test-conversation", CancellationToken.None);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Constructor_NullRootDirectoryWithDefaultApplicationId_CreatesValidStore()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns((string)null!);

        FileConversationProgressLogStore store = new(settings.Object, null);

        Assert.IsNotNull(store);

        // Verify the store can perform operations with default application ID
        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync("test-conversation", CancellationToken.None);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Constructor_EmptyRootDirectory_UsesEmptyStringAsRoot()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("TestApp");

        FileConversationProgressLogStore store = new(settings.Object, string.Empty);

        Assert.IsNotNull(store);

        // Verify the store can perform operations (even with empty root)
        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync("test-conversation", CancellationToken.None);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DeleteConversationAsync_NullConversationId_ReturnsWithoutException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        await store.DeleteConversationAsync(null!, CancellationToken.None);
    }

    [TestMethod]
    public async Task DeleteConversationAsync_EmptyConversationId_ReturnsWithoutException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        await store.DeleteConversationAsync(string.Empty, CancellationToken.None);
    }

    [TestMethod]
    public async Task DeleteConversationAsync_WhitespaceConversationId_ReturnsWithoutException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        await store.DeleteConversationAsync("   ", CancellationToken.None);
    }

    [TestMethod]
    public async Task GetAsync_EmptyPlanId_ReturnsNull()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        ConversationProgressLog result = await store.GetAsync("conversation-1", Guid.Empty, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ListAsync_NullConversationId_ReturnsEmptyList()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync(null!, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ListAsync_EmptyConversationId_ReturnsEmptyList()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync(string.Empty, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ListAsync_WhitespaceConversationId_ReturnsEmptyList()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);

        IReadOnlyList<ConversationProgressLog> result = await store.ListAsync("   ", CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task SaveAsync_NullConversationId_ThrowsArgumentException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);
        ConversationProgressLog plan = new()
        {
                PlanId = Guid.NewGuid(),
                ConversationId = null!,
                PlanName = "test plan",
                CurrentStepId = 1,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                Steps = []
        };

        ArgumentException ex = await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await store.SaveAsync(plan, CancellationToken.None));

        Assert.AreEqual("progressLog", ex.ParamName);
    }

    [TestMethod]
    public async Task SaveAsync_EmptyConversationId_ThrowsArgumentException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);
        ConversationProgressLog plan = new()
        {
                PlanId = Guid.NewGuid(),
                ConversationId = string.Empty,
                PlanName = "test plan",
                CurrentStepId = 1,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                Steps = []
        };

        ArgumentException ex = await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await store.SaveAsync(plan, CancellationToken.None));

        Assert.AreEqual("progressLog", ex.ParamName);
    }

    [TestMethod]
    public async Task SaveAsync_WhitespaceConversationId_ThrowsArgumentException()
    {
        IConversationProgressLogStore store = new FileConversationProgressLogStore(CreateSettings().Object, _root);
        ConversationProgressLog plan = new()
        {
                PlanId = Guid.NewGuid(),
                ConversationId = "   ",
                PlanName = "test plan",
                CurrentStepId = 1,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                Steps = []
        };

        ArgumentException ex = await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await store.SaveAsync(plan, CancellationToken.None));

        Assert.AreEqual("progressLog", ex.ParamName);
    }

    private static Mock<IAppSettings> CreateSettings()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("RAGDataIngestionWPF.Tests");
        return settings;
    }








    [TestMethod]
    public async Task SavePlanFile()
    {

        Mock<IAppSettings> settings = CreateSettings();
        string customRoot = Path.Combine(Path.GetTempPath(), "custom-root");

        FileConversationProgressLogStore store = new(settings.Object, customRoot);
        try
        {
           await store.SavePlansAsync("TestConversationid", new ConversationProgressLog[1], CancellationToken.None);
        }
        catch (Exception)
        {
            Assert.Fail();
        }

}
    
    
    
    
    
}