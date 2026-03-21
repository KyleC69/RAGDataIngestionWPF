// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 051905



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable, INavigationAware
{
    private readonly IChatConversationService _chatConversationService;
    private CancellationTokenSource _responseCancellationTokenSource;
    private bool _historyLoaded;
    [ObservableProperty] private TaskPlanDisplayItem selectedTaskPlan;

    //Running Token counts for different categories
    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int totalTokenCount;




    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessageDisplayItem>();
        TaskPlans = new ObservableCollection<TaskPlanDisplayItem>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);

        _chatConversationService.BusyStateChanged += OnBusyStateChange;



    }








    private void OnBusyStateChange(object sender, bool e)
    {
        IsBusy = e;
    }








    public IRelayCommand CancelMessageCommand { get; }

    public bool IsBusy
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
                CancelMessageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string MessageInput
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    } = string.Empty;

    public ObservableCollection<ChatMessageDisplayItem> Messages { get; }

    public ObservableCollection<TaskPlanDisplayItem> TaskPlans { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }








    /// <inheritdoc />
    public void Dispose()
    {

    }






    /// <inheritdoc />
    public void OnNavigatedFrom()
    {
    }






    /// <inheritdoc />
    public async void OnNavigatedTo(object parameter)
    {
        if (_historyLoaded)
        {
            return;
        }

        _historyLoaded = true;
        try
        {
            IReadOnlyList<ChatMessage> historyMessages = await _chatConversationService
                    .LoadConversationHistoryAsync()
                    .ConfigureAwait(true);

            Messages.Clear();
            foreach (ChatMessage historyMessage in historyMessages)
            {
                Messages.Add(CreateUiMessage(historyMessage));
            }

            await RefreshTaskPlansAsync().ConfigureAwait(true);

            TotalTokenCount = _chatConversationService.ContextTokenCount;
            SessionTokenCount = _chatConversationService.SessionTokenCount;
            RagTokenCount = _chatConversationService.RagTokenCount;
            ToolTokenCount = _chatConversationService.ToolTokenCount;
            SystemTokenCount = _chatConversationService.SystemTokenCount;
        }
        catch
        {
            Messages.Clear();
        }
    }








    private bool CanCancelMessage()
    {
        return IsBusy;
    }








    private void CancelMessage()
    {
        _responseCancellationTokenSource?.Cancel();
    }








    private bool CanSendMessage()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(MessageInput);
    }








    private static ChatMessageDisplayItem CreateUiMessage(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return ChatMessageDisplayItem.Create(message.Role, message.Text);
    }








    private async Task SendMessageAsync()
    {
        //TODO: Need to link to lifecycle of view model and application lifetime.
        _responseCancellationTokenSource = new CancellationTokenSource();


        var content = MessageInput.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        //Add Users message to UI collection
        Messages.Add(ChatMessageDisplayItem.Create(ChatRole.User, content));

        //Clear UI input
        MessageInput = string.Empty;

  

        try
        {
            ChatMessage assistantMessage = await _chatConversationService.SendRequestToModelAsync(content, _responseCancellationTokenSource.Token);
            Messages.Add(CreateUiMessage(assistantMessage));
        }
        catch (OperationCanceledException)
        {
            Messages.Add(ChatMessageDisplayItem.Create(ChatRole.Assistant, "Response cancelled."));
        }
        finally
        {
            _responseCancellationTokenSource?.Dispose();
            _responseCancellationTokenSource = null;
            await RefreshTaskPlansAsync().ConfigureAwait(true);
            TotalTokenCount = _chatConversationService.ContextTokenCount;
            SessionTokenCount= _chatConversationService.SessionTokenCount;
            RagTokenCount = _chatConversationService.RagTokenCount;
            ToolTokenCount = _chatConversationService.ToolTokenCount;
            SystemTokenCount = _chatConversationService.SystemTokenCount;
        }



    }

    private async Task RefreshTaskPlansAsync()
    {
        IReadOnlyList<ConversationProgressLog> plans = await _chatConversationService
            .LoadTaskPlansAsync()
            .ConfigureAwait(true);

        TaskPlans.Clear();
        foreach (ConversationProgressLog plan in plans)
        {
            TaskPlans.Add(TaskPlanDisplayItem.Create(plan));
        }

        SelectedTaskPlan = TaskPlans.FirstOrDefault();
    }
}