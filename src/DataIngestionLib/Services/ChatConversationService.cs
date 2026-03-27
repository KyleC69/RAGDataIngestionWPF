// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 073001



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
///     Encapsulates the management of the agent operations.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{
    private readonly IConversationAgentRunner _agentRunner;
    private readonly IAppSettings _appSettings;
    private readonly IConversationBudgetEvaluator _budgetEvaluator;
    private readonly IConversationBudgetEventPublisher _budgetEventPublisher;
    private readonly IChatBusyStateScopeFactory _busyStateScopeFactory;
    private readonly IConversationHistoryLoader _historyLoader;
    private readonly ILogger<ChatConversationService> _logger;
    private readonly IConversationProgressLogService? _progressLogService;
    private readonly IConversationSessionBootstrapper _sessionBootstrapper;
    private readonly IConversationTokenCounter _tokenCounter;
    private ConversationSessionContext? _sessionContext;
    private const int AutomaticTaskPlanNameLength = 72;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings, IConversationAgentRunner? agentRunner = null, IConversationProgressLogService? progressLogService = null, ISQLChatHistoryProvider? sqlChatHistoryProvider = null) : this(factory, settings, new ConversationSessionBootstrapper(agentFactory, settings, sqlChatHistoryProvider), new ConversationHistoryLoader(settings, sqlChatHistoryProvider), new ConversationTokenCounter(), new ConversationBudgetEvaluator(), new ChatBusyStateScopeFactory(), new ConversationBudgetEventPublisher(), agentRunner ?? new ConversationAgentRunner(), progressLogService)
    {
        ArgumentNullException.ThrowIfNull(agentFactory);
    }








    public ChatConversationService(ILoggerFactory factory, IAppSettings settings, IConversationSessionBootstrapper sessionBootstrapper, IConversationHistoryLoader historyLoader, IConversationTokenCounter tokenCounter, IConversationBudgetEvaluator budgetEvaluator, IChatBusyStateScopeFactory busyStateScopeFactory, IConversationBudgetEventPublisher budgetEventPublisher, IConversationAgentRunner agentRunner, IConversationProgressLogService? progressLogService = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(sessionBootstrapper);
        ArgumentNullException.ThrowIfNull(historyLoader);
        ArgumentNullException.ThrowIfNull(tokenCounter);
        ArgumentNullException.ThrowIfNull(budgetEvaluator);
        ArgumentNullException.ThrowIfNull(busyStateScopeFactory);
        ArgumentNullException.ThrowIfNull(budgetEventPublisher);
        ArgumentNullException.ThrowIfNull(agentRunner);

        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _sessionBootstrapper = sessionBootstrapper;
        _historyLoader = historyLoader;
        _tokenCounter = tokenCounter;
        _budgetEvaluator = budgetEvaluator;
        _busyStateScopeFactory = busyStateScopeFactory;
        _budgetEventPublisher = budgetEventPublisher;
        _agentRunner = agentRunner;
        _progressLogService = progressLogService;
        _logger = factory.CreateLogger<ChatConversationService>();
    }








    /// <summary>
    ///     A collection of settings to provide the token budget allocated for the model
    /// </summary>
    private TokenBudget ConversationTokenBudget { get; }

    public HistoryIdentity HistoryIdentity { get; set; } = new();

    public bool Initialized { get; set; }

    public string ConversationId { get; private set; } = string.Empty;

    /// <summary>
    ///     Internal tracking history of the conversation with the LLM, used for calculating token usage and providing context
    ///     to the LLM. Not intended to be a full record of the conversation, but rather a window into the recent history that
    ///     is relevant for generating responses.
    ///     This allows for more efficient token usage while still maintaining enough context for coherent conversations.
    ///     This is actually managed by the sqlChatHistoryProvider and the Context Injectors.
    /// </summary>
    public List<ChatMessage> AIHistory { get; } = [];

    /// <summary>
    ///     An estimate of the token count in the current conversation. TODO: will be moved to TokenBudget class for source of
    ///     truth
    /// </summary>
    public int ContextTokenCount { get; private set; }

    /// <inheritdoc />
    public int SessionTokenCount { get; private set; }

    /// <inheritdoc />
    public int ToolTokenCount { get; private set; }

    /// <inheritdoc />
    public int RagTokenCount { get; private set; }

    /// <inheritdoc />
    public int SystemTokenCount { get; private set; }








    /// <summary>
    ///     Sends request to LLM and waits for a response.
    /// </summary>
    /// <param name="content">The user message content to answer.</param>
    /// <param name="token">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token)
    {
        ConversationSessionContext? sessionContext = await EnsureSessionContextAsync(token).ConfigureAwait(false);
        using IDisposable busyScope = _busyStateScopeFactory.Enter(busy => BusyStateChanged?.Invoke(this, busy));
        UsageDetails? usageDetails = null;
        Guid? planId = null;
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("User message cannot be empty.", nameof(content));
            }

            if (sessionContext is null)
            {
                throw new InvalidOperationException("Agent session is not initialized.");
            }

            planId = await TryStartAutomaticTaskPlanAsync(content, token).ConfigureAwait(false);
            AIHistory.Add(new ChatMessage(ChatRole.User, content));
            await TryRecordAutomaticTaskPlanArtifactAsync(planId, "user_request", content, token).ConfigureAwait(false);
            await TryMoveAutomaticTaskPlanToStepAsync(planId, 2, token).ConfigureAwait(false);

            ConversationAgentRunResult response = await _agentRunner.RunAsync(sessionContext.Agent, content, sessionContext.Session, token).ConfigureAwait(false);

            usageDetails = response.UsageDetails;
            if (usageDetails is not null)
            {
                _logger.LogUsages(usageDetails.InputTokenCount, usageDetails.CachedInputTokenCount, usageDetails.OutputTokenCount, usageDetails.ReasoningTokenCount, usageDetails.AdditionalCounts, usageDetails.TotalTokenCount);
            }

            var assistantText = response.Text?.Trim() ?? string.Empty;

            ChatMessage msg = new(ChatRole.Assistant, assistantText);
            AIHistory.Add(msg);
            await TryMoveAutomaticTaskPlanToStepAsync(planId, 3, token).ConfigureAwait(false);
            await TryRecordAutomaticTaskPlanArtifactAsync(planId, "assistant_response", assistantText, token).ConfigureAwait(false);
            await TryCompleteAutomaticTaskPlanAsync(planId, token).ConfigureAwait(false);

            return msg;
        }
        catch (OperationCanceledException)
        {
            await TryAbandonAutomaticTaskPlanAsync(planId, "cancelled", token).ConfigureAwait(false);
            throw;
        }
        catch (Exception ex)
        {
            await TryRecordAutomaticTaskPlanArtifactAsync(planId, "last_error", ex.Message, token).ConfigureAwait(false);
            await TryAbandonAutomaticTaskPlanAsync(planId, "failed", token).ConfigureAwait(false);
            throw;
        }
        finally
        {
            UpdateTokenCounts(usageDetails);
            PublishTokenCounts();
        }
    }








    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        ConversationSessionContext? sessionContext = await EnsureSessionContextAsync(CancellationToken.None).ConfigureAwait(false);





        if (sessionContext is null)
        {
            AIHistory.Clear();
            UpdateTokenCounts(null);
            return [];
        }

        var conversationId = sessionContext.ConversationId;
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            AIHistory.Clear();
            UpdateTokenCounts(null);
            return [];
        }

        ConversationId = conversationId;

        var historyMessages = await _historyLoader.LoadConversationHistoryAsync(HistoryIdentity, token).ConfigureAwait(false);

        AIHistory.Clear();
        AIHistory.AddRange(historyMessages);
        UpdateTokenCounts(null);

        return historyMessages;
    }








    public async ValueTask<IReadOnlyList<ConversationProgressLog>> LoadTaskPlansAsync(CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().ListPlansAsync(ConversationId, token).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog?> GetTaskPlanAsync(Guid planId, CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().GetPlanAsync(ConversationId, planId, token).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog> StartTaskPlanAsync(string planName, IReadOnlyList<string> stepTitles, CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().CreatePlanAsync(ConversationId, planName, stepTitles, token).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog> UpdateTaskPlanStepAsync(Guid planId, int stepId, ConversationProgressStepStatus status, CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().SetCurrentStepAsync(ConversationId, planId, stepId, status, token).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog> RecordTaskPlanArtifactAsync(Guid planId, string artifactKey, string artifactValue, CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().RecordArtifactAsync(ConversationId, planId, artifactKey, artifactValue, token).ConfigureAwait(false);
    }








    public async ValueTask<ConversationProgressLog> CompleteTaskPlanAsync(Guid planId, CancellationToken token = default)
    {
        return await GetRequiredProgressLogService().CompletePlanAsync(ConversationId, planId, token).ConfigureAwait(false);
    }








    public async ValueTask AbandonTaskPlanAsync(Guid planId, string? reason = null, CancellationToken token = default)
    {
        await GetRequiredProgressLogService().AbandonPlanAsync(ConversationId, planId, reason, token).ConfigureAwait(false);
    }








    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;








    internal static string BuildAutomaticTaskPlanName(string content)
    {
        var normalized = content.Trim();
        if (normalized.Length > AutomaticTaskPlanNameLength)
        {
            normalized = normalized[..AutomaticTaskPlanNameLength].TrimEnd() + "...";
        }

        return $"Chat request: {normalized}";
    }








    internal async ValueTask<ConversationSessionContext?> EnsureSessionContextAsync(CancellationToken cancellationToken)
    {
        if (_sessionContext is not null)
        {
            return _sessionContext;
        }

        if (Initialized)
        {
            return null;
        }

        _sessionContext = await _sessionBootstrapper.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        ConversationId = _sessionContext.ConversationId;
        HistoryIdentity = _sessionContext.Identity;
        Initialized = true;
        return _sessionContext;
    }








    internal IConversationProgressLogService GetRequiredProgressLogService()
    {
        return _progressLogService ?? throw new InvalidOperationException("Task plan tracking is not configured.");
    }








    /// <inheritdoc />
    public event EventHandler<int>? MaximumContextWarning;








    internal void PublishTokenCounts()
    {
        ConversationBudgetEvaluation evaluation = _budgetEvaluator.Evaluate(ContextTokenCount, ConversationTokenBudget);
        _budgetEventPublisher.Publish(evaluation, ContextTokenCount, () => SessionBugetExceeded?.Invoke(this, EventArgs.Empty), () => TokenBudgetExceeded?.Invoke(this, EventArgs.Empty), count => MaximumContextWarning?.Invoke(this, count));
    }








    /// <inheritdoc />
    public event EventHandler? SessionBugetExceeded;

    /// <inheritdoc />
    public event EventHandler? TokenBudgetExceeded;








    internal async ValueTask TryAbandonAutomaticTaskPlanAsync(Guid? planId, string reason, CancellationToken cancellationToken)
    {
        if (!planId.HasValue || _progressLogService is null)
        {
            return;
        }

        try
        {
            await _progressLogService.AbandonPlanAsync(ConversationId, planId.Value, reason, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to abandon automatic task plan for chat request.");
        }
    }








    internal async ValueTask TryCompleteAutomaticTaskPlanAsync(Guid? planId, CancellationToken cancellationToken)
    {
        if (!planId.HasValue || _progressLogService is null)
        {
            return;
        }

        try
        {
            await _progressLogService.CompletePlanAsync(ConversationId, planId.Value, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to complete automatic task plan for chat request.");
        }
    }








    internal async ValueTask TryMoveAutomaticTaskPlanToStepAsync(Guid? planId, int stepId, CancellationToken cancellationToken)
    {
        if (!planId.HasValue || _progressLogService is null)
        {
            return;
        }

        try
        {
            await _progressLogService.SetCurrentStepAsync(ConversationId, planId.Value, stepId, ConversationProgressStepStatus.InProgress, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to update automatic task plan step for chat request.");
        }
    }








    internal async ValueTask TryRecordAutomaticTaskPlanArtifactAsync(Guid? planId, string artifactKey, string artifactValue, CancellationToken cancellationToken)
    {
        if (!planId.HasValue || _progressLogService is null)
        {
            return;
        }

        try
        {
            await _progressLogService.RecordArtifactAsync(ConversationId, planId.Value, artifactKey, artifactValue, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to record automatic task plan artifact for chat request.");
        }
    }








    internal async ValueTask<Guid?> TryStartAutomaticTaskPlanAsync(string content, CancellationToken cancellationToken)
    {
        if (_progressLogService is null)
        {
            return null;
        }

        try
        {
            ConversationProgressLog plan = await _progressLogService.CreatePlanAsync(ConversationId, BuildAutomaticTaskPlanName(content), ["Queue request", "Run agent", "Finalize response"], cancellationToken).ConfigureAwait(false);
            return plan.PlanId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to start automatic task plan for chat request.");
            return null;
        }
    }








    internal void UpdateTokenCounts(UsageDetails? usageDetails)
    {
        ConversationTokenSnapshot snapshot = _tokenCounter.Calculate(AIHistory, ConversationTokenBudget, usageDetails);

        ContextTokenCount = snapshot.Total;
        SessionTokenCount = snapshot.Session;
        RagTokenCount = snapshot.Rag;
        ToolTokenCount = snapshot.Tool;
        SystemTokenCount = snapshot.System;
    }
}