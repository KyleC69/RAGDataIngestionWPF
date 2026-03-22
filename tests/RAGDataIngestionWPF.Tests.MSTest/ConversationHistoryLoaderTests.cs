// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ConversationHistoryLoaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 140954



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ConversationHistoryLoaderTests
{
    [TestMethod]
    public async Task LoadConversationHistoryMapsPersistedMessagesToChatMessages()
    {
        Guid userMessageId = Guid.NewGuid();
        Guid assistantMessageId = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.Now;

        Mock<ISQLChatHistoryProvider> provider = new();
        provider.Setup(x => x.GetMessagesAsync("conversation-7", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<PersistedChatMessage>
        {
                new()
                {
                        MessageId = userMessageId,
                        ConversationId = "conversation-7",
                        AgentId = "agent",
                        UserId = "user",
                        ApplicationId = "app",
                        Role = ChatRole.User.Value,
                        Content = "first",
                        TimestampUtc = now
                },
                new()
                {
                        MessageId = assistantMessageId,
                        ConversationId = "conversation-7",
                        AgentId = "agent",
                        UserId = "user",
                        ApplicationId = "app",
                        Role = ChatRole.Assistant.Value,
                        Content = "second",
                        TimestampUtc = now.AddMinutes(1)
                }
        });

        ConversationHistoryLoader loader = new(provider.Object);

        var messages = await loader.LoadConversationHistoryAsync("conversation-7", CancellationToken.None);

        Assert.AreEqual(2, messages.Count);
        Assert.AreEqual(ChatRole.User.Value, messages[0].Role.Value);
        Assert.AreEqual("first", messages[0].Text);
        Assert.AreEqual(userMessageId.ToString("D"), messages[0].MessageId);
        Assert.AreEqual(ChatRole.Assistant.Value, messages[1].Role.Value);
        Assert.AreEqual("second", messages[1].Text);
        Assert.AreEqual(assistantMessageId.ToString("D"), messages[1].MessageId);
    }








    [TestMethod]
    public async Task LoadConversationHistoryReturnsEmptyWhenProviderMissing()
    {
        ConversationHistoryLoader loader = new();

        var messages = await loader.LoadConversationHistoryAsync("conversation-7", CancellationToken.None);

        Assert.AreEqual(0, messages.Count);
    }
}