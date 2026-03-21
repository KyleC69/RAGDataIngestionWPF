using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class LocalRagContextOrchestratorTests
{
    private static Mock<IAppSettings> CreateSettings(int ragBudget = 1000)
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.RAGBudget).Returns(ragBudget);
        return settings;
    }

    private static Mock<IRagQueryExpander> CreateQueryExpander(params RagSearchQuery[] queries)
    {
        Mock<IRagQueryExpander> expander = new();
        expander
            .Setup(x => x.Expand(It.IsAny<IReadOnlyList<ChatMessage>>()))
            .Returns(queries);
        return expander;
    }

    [TestMethod]
    public async Task BuildContextMessagesUsesLatestNonEmptyRequestTextAndDeduplicatesIds()
    {
        Mock<IRagRetrievalService> retrieval = new();
        retrieval
            .Setup(x => x.SearchAsync(new RagSearchQuery("latest"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RagSearchResult>
            {
                new(2, "Second", "Summary 2", new[] { "b" }, 0.6),
                new(1, "First", "Summary 1", new[] { "a" }, 0.9),
                new(1, "First Duplicate", "Duplicate summary", new[] { "a" }, 0.1)
            });

        Mock<IRagQueryExpander> expander = CreateQueryExpander(new RagSearchQuery("latest"));
        LocalRagContextOrchestrator orchestrator = new(retrieval.Object, expander.Object, new ContextCitationFormatter(), CreateSettings().Object);
        IReadOnlyList<ChatMessage> requestMessages =
        [
            new(ChatRole.User, "older"),
            new(ChatRole.Assistant, ""),
            new(ChatRole.User, "latest")
        ];

        IReadOnlyList<ChatMessage> contextMessages = await orchestrator.BuildContextMessagesAsync(requestMessages, CancellationToken.None);

        Assert.AreEqual(1, contextMessages.Count);
        Assert.AreEqual("rag_context", contextMessages[0].Role.Value);
        StringAssert.Contains(contextMessages[0].Text, "Relevant local knowledge:");
        StringAssert.Contains(contextMessages[0].Text, "source=local-rag");
        StringAssert.Contains(contextMessages[0].Text, "First");
        StringAssert.Contains(contextMessages[0].Text, "Second");
        Assert.IsFalse(contextMessages[0].Text.Contains("First Duplicate", StringComparison.Ordinal));
        expander.Verify(x => x.Expand(It.Is<IReadOnlyList<ChatMessage>>(messages => messages.Count == 3 && messages[2].Text == "latest")), Times.Once);
        retrieval.Verify(x => x.SearchAsync(It.Is<RagSearchQuery>(query => query.Query == "latest" && query.Mode == RagSearchMode.Hybrid && query.TopK == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task BuildContextMessagesRespectsCharacterBudget()
    {
        string largeSummary = new('x', 600);
        Mock<IRagRetrievalService> retrieval = new();
        retrieval
            .Setup(x => x.SearchAsync(new RagSearchQuery("latest"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RagSearchResult>
            {
                new(1, "First", largeSummary, new[] { "a" }, 0.9),
                new(2, "Second", largeSummary, new[] { "b" }, 0.8)
            });

        LocalRagContextOrchestrator orchestrator = new(retrieval.Object, CreateQueryExpander(new RagSearchQuery("latest")).Object, new ContextCitationFormatter(), CreateSettings(ragBudget: 125).Object);

        IReadOnlyList<ChatMessage> contextMessages = await orchestrator.BuildContextMessagesAsync([new(ChatRole.User, "latest")], CancellationToken.None);

        Assert.AreEqual(1, contextMessages.Count);
        StringAssert.Contains(contextMessages[0].Text, "First");
        Assert.IsFalse(contextMessages[0].Text.Contains("Second", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task BuildContextMessagesFusesExpandedQueriesByRank()
    {
        Mock<IRagRetrievalService> retrieval = new();
        retrieval
            .Setup(x => x.SearchAsync(new RagSearchQuery("latest schema question"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RagSearchResult>
            {
                new(2, "Second", "Summary 2", new[] { "schema" }, 0.95),
                new(1, "First", "Summary 1", new[] { "question" }, 0.70)
            });
        retrieval
            .Setup(x => x.SearchAsync(new RagSearchQuery("schema question"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RagSearchResult>
            {
                new(1, "First", "Summary 1", new[] { "schema" }, 0.60),
                new(3, "Third", "Summary 3", new[] { "schema" }, 0.50)
            });

        LocalRagContextOrchestrator orchestrator = new(
            retrieval.Object,
            CreateQueryExpander(new RagSearchQuery("latest schema question"), new RagSearchQuery("schema question")).Object,
            new ContextCitationFormatter(),
            CreateSettings().Object);

        IReadOnlyList<ChatMessage> contextMessages = await orchestrator.BuildContextMessagesAsync([new(ChatRole.User, "latest schema question")], CancellationToken.None);

        Assert.AreEqual(1, contextMessages.Count);
        int firstIndex = contextMessages[0].Text.IndexOf("First", StringComparison.Ordinal);
        int secondIndex = contextMessages[0].Text.IndexOf("Second", StringComparison.Ordinal);
        Assert.IsTrue(firstIndex >= 0);
        Assert.IsTrue(secondIndex >= 0);
        Assert.IsTrue(firstIndex < secondIndex);
    }
}