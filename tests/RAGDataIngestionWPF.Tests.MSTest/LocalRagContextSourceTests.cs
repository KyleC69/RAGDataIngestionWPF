using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Providers;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class LocalRagContextSourceTests
{
    [TestMethod]
    public async Task GetContextMessagesUsesLatestNonEmptyRequestText()
    {
        Mock<IRagContextOrchestrator> orchestrator = new();
        orchestrator
            .Setup(x => x.BuildContextMessagesAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "context body")
            ]);

        LocalRagContextSource source = new(orchestrator.Object, NullLogger<LocalRagContextSource>.Instance);
        List<ChatMessage> requestMessages =
        [
            new(ChatRole.User, "first"),
            new(ChatRole.Assistant, string.Empty),
            new(ChatRole.User, "latest question")
        ];

        List<ChatMessage> contextMessages = await source.GetContextMessagesAsync(requestMessages, null, CancellationToken.None);

        Assert.AreEqual(1, contextMessages.Count);
        Assert.AreEqual(AIChatRole.RAGContext.Value, contextMessages[0].Role.Value);
        Assert.AreEqual("context body", contextMessages[0].Text);
        orchestrator.Verify(
            x => x.BuildContextMessagesAsync(
                It.Is<IReadOnlyList<ChatMessage>>(messages => messages.Count == 3 && messages[2].Text == "latest question"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetContextMessagesReturnsEmptyWhenNoSearchResults()
    {
        Mock<IRagContextOrchestrator> orchestrator = new();
        orchestrator
            .Setup(x => x.BuildContextMessagesAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        LocalRagContextSource source = new(orchestrator.Object, NullLogger<LocalRagContextSource>.Instance);

        List<ChatMessage> contextMessages = await source.GetContextMessagesAsync([new(ChatRole.User, "question")], null, CancellationToken.None);

        Assert.AreEqual(0, contextMessages.Count);
    }
}