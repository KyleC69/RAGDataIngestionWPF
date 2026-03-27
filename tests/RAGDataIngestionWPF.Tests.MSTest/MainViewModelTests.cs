// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         MainViewModelTests.cs
// Author: Kyle L. Crowder
// Build Num: 073059



using System.Collections.Specialized;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

using Moq;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class MainViewModelTests
{

    [TestMethod]
    public void BusyStateChangedEventUpdatesBusyAndCommandExecutability()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "send" };

        chatConversationServiceMock.Raise(service => service.BusyStateChanged += null, chatConversationServiceMock.Object, true);

        Assert.IsTrue(viewModel.IsBusy);
        Assert.IsFalse(viewModel.SendMessageCommand.CanExecute(null));
        Assert.IsTrue(viewModel.CancelMessageCommand.CanExecute(null));

        chatConversationServiceMock.Raise(service => service.BusyStateChanged += null, chatConversationServiceMock.Object, false);

        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.SendMessageCommand.CanExecute(null));
        Assert.IsFalse(viewModel.CancelMessageCommand.CanExecute(null));
    }








    [TestMethod]
    public void ConstructorWithNullServiceThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new MainViewModel(null!));
    }








    [TestMethod]
    public void OnNavigatedToLoadsPersistedConversationIntoUi()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        IReadOnlyList<ChatMessage> persistedMessages =
        [
                new ChatMessage(ChatRole.User, "previous question"),
                new ChatMessage(ChatRole.Assistant, "previous answer")
        ];
        IReadOnlyList<ConversationProgressLog> taskPlans =
        [
                new ConversationProgressLog
                {
                        ConversationId = "conversation-1",
                        PlanId = Guid.NewGuid(),
                        PlanName = "Refactor chat service",
                        CurrentStepId = 2,
                        Status = ConversationProgressStatus.InProgress,
                        Steps =
                        [
                                new ConversationProgressStep { Id = 1, Title = "Extract seams", Status = ConversationProgressStepStatus.Completed },
                                new ConversationProgressStep { Id = 2, Title = "Wire UI", Status = ConversationProgressStepStatus.InProgress }
                        ],
                        Artifacts = new Dictionary<string, string> { ["notes"] = "ready" },
                        UpdatedAtUtc = new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero)
                }
        ];

        chatConversationServiceMock.Setup(service => service.LoadConversationHistoryAsync(It.IsAny<CancellationToken>())).Returns(new ValueTask<IReadOnlyList<ChatMessage>>(persistedMessages));
        chatConversationServiceMock.Setup(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>())).Returns(new ValueTask<IReadOnlyList<ConversationProgressLog>>(taskPlans));
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(22);
        chatConversationServiceMock.SetupGet(service => service.SessionTokenCount).Returns(11);
        chatConversationServiceMock.SetupGet(service => service.RagTokenCount).Returns(5);
        chatConversationServiceMock.SetupGet(service => service.ToolTokenCount).Returns(4);
        chatConversationServiceMock.SetupGet(service => service.SystemTokenCount).Returns(2);

        MainViewModel viewModel = new(chatConversationServiceMock.Object);

        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual(2, viewModel.Messages.Count);
        Assert.AreEqual("previous question", viewModel.Messages[0].Text);
        Assert.AreEqual("previous answer", viewModel.Messages[1].Text);
        Assert.AreEqual(22, viewModel.TotalTokenCount);
        Assert.AreEqual(11, viewModel.SessionTokenCount);
        Assert.AreEqual(5, viewModel.RagTokenCount);
        Assert.AreEqual(4, viewModel.ToolTokenCount);
        Assert.AreEqual(2, viewModel.SystemTokenCount);
        Assert.AreEqual(1, viewModel.TaskPlans.Count);
        Assert.AreEqual("Refactor chat service", viewModel.TaskPlans[0].PlanName);
        Assert.AreEqual(viewModel.TaskPlans[0], viewModel.SelectedTaskPlan);
    }








    [TestMethod]
    public void SendMessageCommandCannotExecuteForWhitespaceInput()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "   " };

        Assert.IsFalse(viewModel.SendMessageCommand.CanExecute(null));
    }








    [TestMethod]
    public async Task WhenSendMessageCompletesThenAssistantMessageIsProjectedForUiBinding()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(8);
        chatConversationServiceMock.Setup(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>())).Returns(new ValueTask<IReadOnlyList<ConversationProgressLog>>([]));
        chatConversationServiceMock.Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>())).Returns(new ValueTask<ChatMessage>(new ChatMessage(ChatRole.Assistant, "hi there")));

        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "hello" };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual("hi there", viewModel.Messages[1].Text);
        Assert.IsFalse(viewModel.Messages[1].IsUser);
        Assert.AreEqual(ChatRole.Assistant.ToString(), viewModel.Messages[1].Role);
    }








    [TestMethod]
    public async Task WhenSendMessageCompletesThenMessagesCollectionRaisesAddNotifications()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(8);
        chatConversationServiceMock.Setup(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>())).Returns(new ValueTask<IReadOnlyList<ConversationProgressLog>>([]));
        chatConversationServiceMock.Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>())).Returns(new ValueTask<ChatMessage>(new ChatMessage(ChatRole.Assistant, "hi there")));

        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "hello" };
        var addNotificationCount = 0;
        viewModel.Messages.CollectionChanged += (_, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                addNotificationCount += args.NewItems?.Count ?? 0;
            }
        };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual(2, addNotificationCount);
    }








    [TestMethod]
    public async Task WhenSendMessageCompletesThenTaskPlansAreRefreshed()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(8);
        chatConversationServiceMock.Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>())).Returns(new ValueTask<ChatMessage>(new ChatMessage(ChatRole.Assistant, "hi there")));
        chatConversationServiceMock.Setup(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<IReadOnlyList<ConversationProgressLog>>([
                        new ConversationProgressLog
                        {
                                ConversationId = "conversation-1",
                                PlanId = Guid.NewGuid(),
                                PlanName = "Refresh plans",
                                CurrentStepId = 1,
                                Steps = [new ConversationProgressStep { Id = 1, Title = "Update", Status = ConversationProgressStepStatus.InProgress }]
                        }
                ]));

        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "hello" };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual(1, viewModel.TaskPlans.Count);
        Assert.AreEqual("Refresh plans", viewModel.TaskPlans[0].PlanName);
        chatConversationServiceMock.Verify(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }








    [TestMethod]
    public async Task WhenSendMessageIsCancelledThenCancellationMessageIsAdded()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(14);
        chatConversationServiceMock.SetupGet(service => service.SessionTokenCount).Returns(6);
        chatConversationServiceMock.SetupGet(service => service.RagTokenCount).Returns(3);
        chatConversationServiceMock.SetupGet(service => service.ToolTokenCount).Returns(2);
        chatConversationServiceMock.SetupGet(service => service.SystemTokenCount).Returns(1);
        chatConversationServiceMock.Setup(service => service.LoadTaskPlansAsync(It.IsAny<CancellationToken>())).Returns(new ValueTask<IReadOnlyList<ConversationProgressLog>>([]));
        chatConversationServiceMock.Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException());

        MainViewModel viewModel = new(chatConversationServiceMock.Object) { MessageInput = "hello" };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual(2, viewModel.Messages.Count);
        Assert.AreEqual("Response cancelled.", viewModel.Messages[1].Text);
        Assert.AreEqual(14, viewModel.TotalTokenCount);
        Assert.AreEqual(6, viewModel.SessionTokenCount);
        Assert.AreEqual(3, viewModel.RagTokenCount);
        Assert.AreEqual(2, viewModel.ToolTokenCount);
        Assert.AreEqual(1, viewModel.SystemTokenCount);
    }
}