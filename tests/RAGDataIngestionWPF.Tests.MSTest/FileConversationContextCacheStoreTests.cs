using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class FileConversationContextCacheStoreTests
{
    private string _root = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "conversation-cache-tests", Guid.NewGuid().ToString("N"));
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
    public async Task AppendAsyncDeduplicatesAndPersistsEntries()
    {
        IConversationContextCacheStore store = new FileConversationContextCacheStore(CreateSettings().Object, _root);

        await store.AppendAsync(
            "conversation-1",
            [
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "cached block"),
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "cached block"),
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "other block")
            ],
            CancellationToken.None);

        IReadOnlyList<ConversationContextCacheEntry> results = await store.SearchAsync("conversation-1", "cached", 5, CancellationToken.None);

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("cached block", results[0].Text);
    }

    [TestMethod]
    public async Task SearchAsyncReturnsMostRelevantNewestEntries()
    {
        IConversationContextCacheStore store = new FileConversationContextCacheStore(CreateSettings().Object, _root);

        await store.AppendAsync(
            "conversation-1",
            [
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "schema validation old note")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)
                },
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "startup schema validation latest note")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero)
                },
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "unrelated")
                {
                    CreatedAt = new DateTimeOffset(2026, 3, 3, 0, 0, 0, TimeSpan.Zero)
                }
            ],
            CancellationToken.None);

        IReadOnlyList<ConversationContextCacheEntry> results = await store.SearchAsync("conversation-1", "startup schema validation", 2, CancellationToken.None);

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual("startup schema validation latest note", results[0].Text);
        Assert.AreEqual("schema validation old note", results[1].Text);
    }

    [TestMethod]
    public async Task ResetAsyncDeletesConversationCache()
    {
        IConversationContextCacheStore store = new FileConversationContextCacheStore(CreateSettings().Object, _root);

        await store.AppendAsync("conversation-1", [new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), "cached block")], CancellationToken.None);
        await store.ResetAsync("conversation-1", CancellationToken.None);

        IReadOnlyList<ConversationContextCacheEntry> results = await store.SearchAsync("conversation-1", "cached", 5, CancellationToken.None);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task AppendAsyncSkipsMessagesThatAreNotContextOrToolRoles()
    {
        IConversationContextCacheStore store = new FileConversationContextCacheStore(CreateSettings().Object, _root);

        await store.AppendAsync(
            "conversation-1",
            [
                new ChatMessage(ChatRole.Assistant, "assistant response should not be cached"),
                new ChatMessage(new ChatRole(AIChatRole.AIContext.Value), "history context"),
                new ChatMessage(new ChatRole(AIChatRole.Tool.Value), "tool result")
            ],
            CancellationToken.None);

        IReadOnlyList<ConversationContextCacheEntry> contextResults = await store.SearchAsync("conversation-1", "history", 5, CancellationToken.None);
        IReadOnlyList<ConversationContextCacheEntry> toolResults = await store.SearchAsync("conversation-1", "tool", 5, CancellationToken.None);
        IReadOnlyList<ConversationContextCacheEntry> assistantResults = await store.SearchAsync("conversation-1", "assistant", 5, CancellationToken.None);

        Assert.AreEqual(1, contextResults.Count);
        Assert.AreEqual(AIChatRole.AIContext.Value, contextResults[0].Role);
        Assert.AreEqual(1, toolResults.Count);
        Assert.AreEqual(AIChatRole.Tool.Value, toolResults[0].Role);
        Assert.AreEqual(0, assistantResults.Count);
    }

    private static Mock<IAppSettings> CreateSettings()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.ApplicationId).Returns("RAGDataIngestionWPF.Tests");
        return settings;
    }
}