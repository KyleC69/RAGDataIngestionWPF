using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ConversationHistoryContextOrchestratorTests
{
    private static Mock<IAppSettings> CreateSettings(int metaBudget = 1000)
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.MetaBudget).Returns(metaBudget);
        return settings;
    }

    [TestMethod]
    public async Task BuildContextMessagesReturnsRankedRelevantHistory()
    {
        Mock<IConversationHistoryLoader> loader = new();
        loader
            .Setup(x => x.LoadConversationHistoryAsync("conversation-42", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ChatMessage(ChatRole.Assistant, "General setup note")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 10, 8, 0, 0, TimeSpan.Zero)
                },
                new ChatMessage(ChatRole.Assistant, "Schema validation should run during startup initialization.")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 11, 8, 0, 0, TimeSpan.Zero)
                },
                new ChatMessage(ChatRole.User, "We should review schema validation for startup.")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 12, 8, 0, 0, TimeSpan.Zero)
                }
            ]);

        ConversationHistoryContextOrchestrator orchestrator = new(loader.Object, new ContextCitationFormatter(), CreateSettings().Object);

        IReadOnlyList<ChatMessage> messages = await orchestrator.BuildContextMessagesAsync(
            "conversation-42",
            [new(ChatRole.User, "How does schema validation work at startup?")],
            CancellationToken.None);

        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(AIChatRole.RAGContext.Value, messages[0].Role.Value);
        StringAssert.Contains(messages[0].Text, "Relevant conversation history:");
        StringAssert.Contains(messages[0].Text, "source=conversation-history");
        StringAssert.Contains(messages[0].Text, "Schema validation should run during startup initialization.");
        StringAssert.Contains(messages[0].Text, "We should review schema validation for startup.");
        Assert.IsFalse(messages[0].Text.Contains("General setup note", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task BuildContextMessagesSkipsHistoryThatDuplicatesRequestText()
    {
        Mock<IConversationHistoryLoader> loader = new();
        loader
            .Setup(x => x.LoadConversationHistoryAsync("conversation-42", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ChatMessage(ChatRole.User, "Repeat this exact request"),
                new ChatMessage(ChatRole.Assistant, "Related answer for this exact request")
            ]);

        ConversationHistoryContextOrchestrator orchestrator = new(loader.Object, new ContextCitationFormatter(), CreateSettings().Object);

        IReadOnlyList<ChatMessage> messages = await orchestrator.BuildContextMessagesAsync(
            "conversation-42",
            [new(ChatRole.User, "Repeat this exact request")],
            CancellationToken.None);

        Assert.AreEqual(1, messages.Count);
        Assert.IsFalse(messages[0].Text.Contains("Repeat this exact request", StringComparison.Ordinal));
        StringAssert.Contains(messages[0].Text, "Related answer for this exact request");
    }

    [TestMethod]
    public async Task BuildContextMessagesRespectsMetaBudget()
    {
        string largeText = new('x', 400);
        Mock<IConversationHistoryLoader> loader = new();
        loader
            .Setup(x => x.LoadConversationHistoryAsync("conversation-42", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ChatMessage(ChatRole.Assistant, $"startup schema validation {largeText}"),
                new ChatMessage(ChatRole.Assistant, $"startup schema validation second {largeText}")
            ]);

        ConversationHistoryContextOrchestrator orchestrator = new(loader.Object, new ContextCitationFormatter(), CreateSettings(metaBudget: 75).Object);

        IReadOnlyList<ChatMessage> messages = await orchestrator.BuildContextMessagesAsync(
            "conversation-42",
            [new(ChatRole.User, "startup schema validation")],
            CancellationToken.None);

        Assert.AreEqual(1, messages.Count);
        Assert.IsTrue(messages[0].Text.Contains("startup schema validation second", StringComparison.Ordinal)
                      || messages[0].Text.Contains("startup schema validation ", StringComparison.Ordinal));
        int blockCount = messages[0].Text.Split("[", StringSplitOptions.RemoveEmptyEntries).Length - 1;
        Assert.AreEqual(1, blockCount);
    }
}