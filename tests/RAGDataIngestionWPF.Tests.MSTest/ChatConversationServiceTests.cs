// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ChatConversationServiceTests.cs
// Author: Kyle L. Crowder
// Build Num: 140948



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ChatConversationServiceTests
{




    // BusyStateChanged event behaviour
    // -------------------------------------------------------------------------















    [TestMethod]
    public void ConstructorWithNullAgentIdentityProviderThrowsArgumentNullException()
    {
        IAppSettings settings = MakeSettingsMock().Object;
        IAgentFactory agentFactory = new Mock<IAgentFactory>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ChatConversationService(NullLoggerFactory.Instance, agentFactory, settings, null!));
    }


    [TestClass]
    public class ChatConversationServiceTests
    {
        // -------------------------------------------------------------------------
        // Constructor guard tests
        // -------------------------------------------------------------------------

        private static ChatConversationService CreateService(IConversationSessionBootstrapper sessionBootstrapper = null, IConversationHistoryLoader historyLoader = null, IConversationAgentRunner agentRunner = null, IConversationProgressLogService progressLogService = null, IAppSettings settings = null)


















    private static ChatConversationService CreateService(IConversationSessionBootstrapper sessionBootstrapper = null, IConversationHistoryLoader historyLoader = null, IConversationAgentRunner agentRunner = null, IConversationProgressLogService progressLogService = null, IAppSettings settings = null)
    {
        sessionBootstrapper ??= Mock.Of<IConversationSessionBootstrapper>();
        historyLoader ??= Mock.Of<IConversationHistoryLoader>();
        settings = settings ?? MakeSettingsMock().Object;
        return new ChatConversationService(NullLoggerFactory.Instance, settings, sessionBootstrapper, historyLoader, new ConversationTokenCounter(), new ConversationBudgetEvaluator(), new ChatBusyStateScopeFactory(), new ConversationBudgetEventPublisher(), agentRunner ?? Mock.Of<IConversationAgentRunner>(), progressLogService);
    }








    // -------------------------------------------------------------------------
    // Initial state / property tests
    // -------------------------------------------------------------------------








    [TestMethod]
    public void InitializedDefaultsToFalse()
    {
        ChatConversationService service = CreateService();

        Assert.IsFalse(service.Initialized);
    }








    [TestMethod]
    public async Task LoadConversationHistoryUsesInjectedLoaderAndUpdatesState()
    {
        var bootstrapper = new Mock<IConversationSessionBootstrapper>();
        bootstrapper.Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationSessionContext(null!, null!, "conversation-42"));

        var expectedHistory = new List<ChatMessage> { new(ChatRole.User, "hello") { MessageId = Guid.NewGuid().ToString("D") }, new(ChatRole.Assistant, "world") { MessageId = Guid.NewGuid().ToString("D") } };

        var historyLoader = new Mock<IConversationHistoryLoader>();
        historyLoader.Setup(x => x.LoadConversationHistoryAsync("conversation-42", It.IsAny<CancellationToken>())).ReturnsAsync(expectedHistory);

        ChatConversationService service = CreateService(bootstrapper.Object, historyLoader.Object);

        var result = await service.LoadConversationHistoryAsync(CancellationToken.None);

        Assert.AreEqual("conversation-42", service.ConversationId);
        Assert.AreEqual(2, service.AIHistory.Count);
        Assert.AreEqual("hello", service.AIHistory[0].Text);
        Assert.AreEqual("world", service.AIHistory[1].Text);
        Assert.AreEqual(2, result.Count);

        bootstrapper.Verify(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>()), Times.Once);
        historyLoader.Verify(x => x.LoadConversationHistoryAsync("conversation-42", It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public async Task LoadTaskPlansDelegatesToProgressLogServiceForConversation()
    {
        var bootstrapper = new Mock<IConversationSessionBootstrapper>();
        bootstrapper.Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationSessionContext(null!, null!, "conversation-42"));

        var historyLoader = new Mock<IConversationHistoryLoader>();
        IReadOnlyList<ConversationProgressLog> expectedPlans =
        [
                new() { ConversationId = "conversation-42", PlanId = Guid.NewGuid(), PlanName = "rewrite" }
        ];

        var progressLogService = new Mock<IConversationProgressLogService>();
        progressLogService.Setup(x => x.ListPlansAsync("conversation-42", It.IsAny<CancellationToken>())).ReturnsAsync(expectedPlans);

        ChatConversationService service = CreateService(bootstrapper.Object, historyLoader.Object, progressLogService: progressLogService.Object);

        var plans = await service.LoadTaskPlansAsync(CancellationToken.None);

        Assert.AreEqual(1, plans.Count);
        Assert.AreEqual("rewrite", plans[0].PlanName);
        progressLogService.Verify(x => x.ListPlansAsync("conversation-42", It.IsAny<CancellationToken>()), Times.Once);
    }
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------








    private static TokenBudget MakeBudget(int sessionBudget = 100_000, int maximumContext = 80_000, int systemBudget = 5_000, int ragBudget = 10_000, int toolBudget = 5_000, int metaBudget = 1_000) => new()
    {
            SessionBudget = sessionBudget,
            MaximumContext = maximumContext,
            SystemBudget = systemBudget,
            RAGBudget = ragBudget,
            ToolBudget = toolBudget,
            MetaBudget = metaBudget,
            BudgetTotal = sessionBudget + systemBudget + ragBudget + toolBudget + metaBudget
    };








    private static Mock<IAppSettings> MakeSettingsMock(string applicationId = "test-app", TokenBudget budget = null)
    {
        var mock = new Mock<IAppSettings>();
        mock.SetupGet(s => s.AgentId).Returns("test-agent");
        mock.SetupGet(s => s.ApplicationId).Returns(applicationId);
        mock.Setup(s => s.GetTokenBudget()).Returns(budget ?? MakeBudget());
        return mock;
    }








    [TestMethod]
    public async Task MaximumContextWarningDoesNotFireWhenBudgetExceeded()
    {
        // When SessionBudget is also hit, SessionBugetExceeded fires and returns early
        // before MaximumContextWarning can be raised.
        TokenBudget budget = MakeBudget(sessionBudget: 1, maximumContext: 1);
        var settings = MakeSettingsMock(budget: budget);
        ChatConversationService service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        var warningFired = false;
        service.MaximumContextWarning += (_, _) => warningFired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsFalse(warningFired, "MaximumContextWarning should NOT fire when SessionBugetExceeded path short-circuits");
    }








    [TestMethod]
    public async Task MaximumContextWarningFiresWhenContextTokensReachMaximumContext()
    {
        // MaximumContext = 1, SessionBudget >> 1: triggers warning but not exceeded.
        TokenBudget budget = MakeBudget(sessionBudget: 100_000, maximumContext: 1);
        var settings = MakeSettingsMock(budget: budget);
        ChatConversationService service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        int? warningTokenCount = null;
        service.MaximumContextWarning += (_, count) => warningTokenCount = count;

        await TriggerTokenUpdateAsync(service);

        Assert.IsNotNull(warningTokenCount, "MaximumContextWarning should have fired");
        Assert.AreEqual(1, warningTokenCount);
    }








    [TestMethod]
    public async Task MessagesExceedingSessionBudgetAreNotCounted()
    {
        // Budget of 2 tokens; add three 1-token messages. The oldest (index 0) should
        // be excluded because iterating from newest: msg[2] (total=1), msg[1] (total=2),
        // msg[0] would make total=3 > 2 so it breaks.
        TokenBudget budget = MakeBudget(sessionBudget: 2, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        ChatConversationService service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // oldest
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd"));
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // newest

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(2, service.ContextTokenCount);
    }








    [TestMethod]
    public async Task NoBudgetEventsFireWhenHistoryIsEmpty()
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        var anyFired = false;
        service.SessionBugetExceeded += (_, _) => anyFired = true;
        service.TokenBudgetExceeded += (_, _) => anyFired = true;
        service.MaximumContextWarning += (_, _) => anyFired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsFalse(anyFired, "No budget events should fire for an empty history");
    }








    [TestMethod]
    [DataRow("rag_context")]
    [DataRow("context")]
    [DataRow("rag")]
    public async Task RagRoleVariantsCountAsRagTokens(string roleValue)
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(roleValue), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.RagTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }








    [TestMethod]
    public async Task SendRequestAbandonsAutomaticTaskPlanWhenCancelled()
    {
        var bootstrapper = new Mock<IConversationSessionBootstrapper>();
        bootstrapper.Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationSessionContext(null!, null!, "conversation-42"));

        var historyLoader = new Mock<IConversationHistoryLoader>();
        var agentRunner = new Mock<IConversationAgentRunner>();
        agentRunner.Setup(x => x.RunAsync(It.IsAny<Microsoft.Agents.AI.AIAgent>(), "hello", It.IsAny<Microsoft.Agents.AI.AgentSession>(), It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException());

        Guid planId = Guid.NewGuid();
        var progressLogService = new Mock<IConversationProgressLogService>();
        progressLogService.Setup(x => x.CreatePlanAsync("conversation-42", It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId, PlanName = "Chat request: hello" });
        progressLogService.Setup(x => x.SetCurrentStepAsync("conversation-42", planId, It.IsAny<int>(), ConversationProgressStepStatus.InProgress, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId });
        progressLogService.Setup(x => x.RecordArtifactAsync("conversation-42", planId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId });
        progressLogService.Setup(x => x.AbandonPlanAsync("conversation-42", planId, "cancelled", It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        ChatConversationService service = CreateService(bootstrapper.Object, historyLoader.Object, agentRunner.Object, progressLogService.Object);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => service.SendRequestToModelAsync("hello", CancellationToken.None).AsTask());

        progressLogService.Verify(x => x.AbandonPlanAsync("conversation-42", planId, "cancelled", It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public async Task SendRequestCreatesAndCompletesAutomaticTaskPlan()
    {
        var bootstrapper = new Mock<IConversationSessionBootstrapper>();
        bootstrapper.Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationSessionContext(null!, null!, "conversation-42"));

        var historyLoader = new Mock<IConversationHistoryLoader>();
        var agentRunner = new Mock<IConversationAgentRunner>();
        agentRunner.Setup(x => x.RunAsync(It.IsAny<Microsoft.Agents.AI.AIAgent>(), "hello", It.IsAny<Microsoft.Agents.AI.AgentSession>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationAgentRunResult("hi there", null));

        Guid planId = Guid.NewGuid();
        var progressLogService = new Mock<IConversationProgressLogService>();
        progressLogService.Setup(x => x.CreatePlanAsync("conversation-42", It.Is<string>(name => name.StartsWith("Chat request:")), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId, PlanName = "Chat request: hello" });
        progressLogService.Setup(x => x.SetCurrentStepAsync("conversation-42", planId, It.IsAny<int>(), ConversationProgressStepStatus.InProgress, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId });
        progressLogService.Setup(x => x.RecordArtifactAsync("conversation-42", planId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId });
        progressLogService.Setup(x => x.CompletePlanAsync("conversation-42", planId, It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = planId, Status = ConversationProgressStatus.Completed });

        ChatConversationService service = CreateService(bootstrapper.Object, historyLoader.Object, agentRunner.Object, progressLogService.Object);

        ChatMessage result = await service.SendRequestToModelAsync("hello", CancellationToken.None);

        Assert.AreEqual("hi there", result.Text);
        Assert.AreEqual(2, service.AIHistory.Count);
        progressLogService.Verify(x => x.CreatePlanAsync("conversation-42", It.IsAny<string>(), It.Is<IReadOnlyList<string>>(steps => steps.Count == 3), It.IsAny<CancellationToken>()), Times.Once);
        progressLogService.Verify(x => x.SetCurrentStepAsync("conversation-42", planId, 2, ConversationProgressStepStatus.InProgress, It.IsAny<CancellationToken>()), Times.Once);
        progressLogService.Verify(x => x.SetCurrentStepAsync("conversation-42", planId, 3, ConversationProgressStepStatus.InProgress, It.IsAny<CancellationToken>()), Times.Once);
        progressLogService.Verify(x => x.RecordArtifactAsync("conversation-42", planId, "user_request", "hello", It.IsAny<CancellationToken>()), Times.Once);
        progressLogService.Verify(x => x.RecordArtifactAsync("conversation-42", planId, "assistant_response", "hi there", It.IsAny<CancellationToken>()), Times.Once);
        progressLogService.Verify(x => x.CompletePlanAsync("conversation-42", planId, It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public async Task SendRequestWhenAgentNotInitializedThrowsInvalidOperationException()
    {
        // Initialized = true means InitializeAsync() is skipped, so _agent stays null.
        ChatConversationService service = CreateService();
        service.Initialized = true;

        try
        {
            await service.SendRequestToModelAsync("hello", CancellationToken.None);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
        }
    }








    // -------------------------------------------------------------------------
    // SendRequestToModelAsync – argument / state validation
    // -------------------------------------------------------------------------








    [TestMethod]
    public async Task SendRequestWithEmptyContentThrowsArgumentException()
    {
        ChatConversationService service = CreateService();
        service.Initialized = true; // bypass InitializeAsync so _agent remains null

        try
        {
            await service.SendRequestToModelAsync(string.Empty, CancellationToken.None);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
        }
    }








    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public async Task SendRequestWithWhitespaceContentThrowsArgumentException(string content)
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        try
        {
            await service.SendRequestToModelAsync(content, CancellationToken.None);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
        }
    }








    // -------------------------------------------------------------------------
    // Budget event tests
    // -------------------------------------------------------------------------








    [TestMethod]
    public async Task SessionBugetExceededFiresWhenContextTokensReachSessionBudget()
    {
        // SessionBudget = 1: a single 4-char message (1 token) fills the budget exactly.
        TokenBudget budget = MakeBudget(sessionBudget: 1, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        ChatConversationService service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        var fired = false;
        service.SessionBugetExceeded += (_, _) => fired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsTrue(fired, "SessionBugetExceeded should fire when context == SessionBudget");
    }








    [TestMethod]
    public async Task StartTaskPlanUsesConversationScopedProgressLogService()
    {
        var bootstrapper = new Mock<IConversationSessionBootstrapper>();
        bootstrapper.Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationSessionContext(null!, null!, "conversation-42"));

        var historyLoader = new Mock<IConversationHistoryLoader>();
        var progressLogService = new Mock<IConversationProgressLogService>();
        progressLogService.Setup(x => x.CreatePlanAsync("conversation-42", "rewrite", It.Is<IReadOnlyList<string>>(steps => steps.Count == 2), It.IsAny<CancellationToken>())).ReturnsAsync(new ConversationProgressLog { ConversationId = "conversation-42", PlanId = Guid.NewGuid(), PlanName = "rewrite" });

        ChatConversationService service = CreateService(bootstrapper.Object, historyLoader.Object, progressLogService: progressLogService.Object);

        ConversationProgressLog plan = await service.StartTaskPlanAsync("rewrite", ["extract seams", "validate"], CancellationToken.None);

        Assert.AreEqual("conversation-42", service.ConversationId);
        Assert.AreEqual("rewrite", plan.PlanName);
        progressLogService.Verify(x => x.CreatePlanAsync("conversation-42", "rewrite", It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public async Task SystemRoleMessageCountsAsSystemTokens()
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(AIChatRole.System.Value), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }








    [TestMethod]
    public async Task TokenBudgetExceededFiresWhenContextTokensReachSessionBudget()
    {
        TokenBudget budget = MakeBudget(sessionBudget: 1, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        ChatConversationService service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        var fired = false;
        service.TokenBudgetExceeded += (_, _) => fired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsTrue(fired, "TokenBudgetExceeded should fire when context == SessionBudget");
    }








    [TestMethod]
    public async Task ToolRoleMessageCountsAsToolTokens()
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(AIChatRole.Tool.Value), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.ToolTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }








    // -------------------------------------------------------------------------
    // Token counting – history-based bucket categorisation
    // -------------------------------------------------------------------------








    /// <summary>
    ///     Triggers UpdateTokenCounts via the finally block of a faulted SendRequest call.
    /// </summary>

    [TestClass]
    public class ChatConversationServiceTests
    {
    private static async Task TriggerTokenUpdateAsync(ChatConversationService service)
    {
        try
        {
            await service.SendRequestToModelAsync("ping", CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
        }
    }








    [TestMethod]
    public async Task UserAndAssistantMessagesCountAsSesionTokens()
    {
        ChatConversationService service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token
        service.AIHistory.Add(new ChatMessage(ChatRole.Assistant, "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(2, service.SessionTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
    }








    [TestMethod]
    public void UserIdReturnsEnvironmentUserName()
    {
        Assert.AreEqual(Environment.UserName, ChatConversationService.UserId);
    }
}