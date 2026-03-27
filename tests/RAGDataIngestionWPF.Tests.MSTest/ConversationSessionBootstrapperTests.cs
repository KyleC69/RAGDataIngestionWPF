// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ConversationSessionBootstrapperTests.cs
// Author: Kyle L. Crowder
// Build Num: 073050



#nullable enable

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ConversationSessionBootstrapperTests
{
    [TestMethod]
    public void ConstructorWithNullAgentFactoryThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConversationSessionBootstrapper(null!, Mock.Of<IAppSettings>()));
    }








    [TestMethod]
    public void ConstructorWithNullAppSettingsThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConversationSessionBootstrapper(Mock.Of<IAgentFactory>(), null!));
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ResolveApplicationIdReturnsFallbackWhenMissing(string configuredApplicationId)
    {
        var appSettings = new Mock<IAppSettings>();
        appSettings.SetupGet(x => x.ApplicationId).Returns(configuredApplicationId);

        ConversationSessionBootstrapper bootstrapper = new(Mock.Of<IAgentFactory>(), appSettings.Object);

        var applicationId = bootstrapper.ResolveApplicationId();

        Assert.AreEqual("unknown-application", applicationId);
    }








    [TestMethod]
    public void ResolveApplicationIdReturnsTrimmedConfiguredValue()
    {
        var appSettings = new Mock<IAppSettings>();
        appSettings.SetupGet(x => x.ApplicationId).Returns("  test-app  ");

        ConversationSessionBootstrapper bootstrapper = new(Mock.Of<IAgentFactory>(), appSettings.Object);

        var applicationId = bootstrapper.ResolveApplicationId();

        Assert.AreEqual("test-app", applicationId);
    }








    [TestMethod]
    public async Task ResolveStartupConversationIdAsyncCreatesNewGuidWhenNoExistingConversationExists()
    {
        var appSettings = new Mock<IAppSettings>();
        appSettings.SetupGet(x => x.LastConversationId).Returns("   ");

        var sqlChatHistoryProvider = new Mock<ISQLChatHistoryProvider>();
        sqlChatHistoryProvider.Setup(x => x.GetLatestConversationIdAsync("agent-1", "user-1", "app-1", It.IsAny<CancellationToken>())).ReturnsAsync(" ");

        ConversationSessionBootstrapper bootstrapper = new(Mock.Of<IAgentFactory>(), appSettings.Object, sqlChatHistoryProvider.Object);

        var conversationId = await bootstrapper.ResolveStartupConversationIdAsync("agent-1", "app-1", "user-1", CancellationToken.None);

        Assert.IsTrue(Guid.TryParseExact(conversationId, "N", out _));
    }








    [TestMethod]
    public async Task ResolveStartupConversationIdAsyncFallsBackToConfiguredConversationId()
    {
        var appSettings = new Mock<IAppSettings>();
        appSettings.SetupGet(x => x.LastConversationId).Returns("  configured-conversation  ");

        var sqlChatHistoryProvider = new Mock<ISQLChatHistoryProvider>();
        sqlChatHistoryProvider.Setup(x => x.GetLatestConversationIdAsync("agent-1", "user-1", "app-1", It.IsAny<CancellationToken>())).ReturnsAsync(default(string));

        ConversationSessionBootstrapper bootstrapper = new(Mock.Of<IAgentFactory>(), appSettings.Object, sqlChatHistoryProvider.Object);

        var conversationId = await bootstrapper.ResolveStartupConversationIdAsync("agent-1", "app-1", "user-1", CancellationToken.None);

        Assert.AreEqual("configured-conversation", conversationId);
    }








    [TestMethod]
    public async Task ResolveStartupConversationIdAsyncUsesLatestConversationIdFromProvider()
    {
        var appSettings = new Mock<IAppSettings>();
        appSettings.SetupGet(x => x.LastConversationId).Returns("settings-conversation");

        var sqlChatHistoryProvider = new Mock<ISQLChatHistoryProvider>();
        sqlChatHistoryProvider.Setup(x => x.GetLatestConversationIdAsync("agent-1", "user-1", "app-1", It.IsAny<CancellationToken>())).ReturnsAsync("  provider-conversation  ");

        ConversationSessionBootstrapper bootstrapper = new(Mock.Of<IAgentFactory>(), appSettings.Object, sqlChatHistoryProvider.Object);

        var conversationId = await bootstrapper.ResolveStartupConversationIdAsync("agent-1", "app-1", "user-1", CancellationToken.None);

        Assert.AreEqual("provider-conversation", conversationId);
        sqlChatHistoryProvider.Verify(x => x.GetLatestConversationIdAsync("agent-1", "user-1", "app-1", It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public void ResolveUserIdReturnsEnvironmentUserName()
    {
        Assert.AreEqual(Environment.UserName, ConversationSessionBootstrapper.ResolveUserId());
    }
}