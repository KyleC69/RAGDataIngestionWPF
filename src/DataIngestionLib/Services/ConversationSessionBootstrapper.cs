// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationSessionBootstrapper.cs
// Author: Kyle L. Crowder
// Build Num: 133603



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationSessionBootstrapper : IConversationSessionBootstrapper, IDisposable
{

    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettings _appSettings;
    private readonly SemaphoreSlim _initializeGate = new SemaphoreSlim(1, 1);
    private readonly ISQLChatHistoryProvider? _sqlChatHistoryProvider;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private string? _conversationId;
    private ProviderSessionState<HistoryIdentity>? _providerSessionState;
    private static readonly TimeSpan INITIALIZATION_LOCK_TIMEOUT = TimeSpan.FromSeconds(10);








    public ConversationSessionBootstrapper(IAgentFactory agentFactory, IAppSettings appSettings, ISQLChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(appSettings);

        _agentFactory = agentFactory;
        _appSettings = appSettings;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
    }








    public async ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized())
        {
            return CreateSessionContext();
        }

        if (!await TryAcquireInitializationLockAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new TimeoutException("Failed to acquire initialization lock within the timeout period.");
        }

        try
        {

            AIAgent agent = _agentFactory.GetCodingAssistantAgent("Agentic-Max", AIModels.GPTOSS, "Agentic-Max Description");
            var applicationId = ResolveApplicationId();
            var userId = ResolveUserId();
            var conversationId = await ResolveStartupConversationIdAsync(agent.Id, applicationId, userId, cancellationToken).ConfigureAwait(false);
            AgentSession session = await agent.CreateSessionAsync().ConfigureAwait(false);

            InitializeProviderSessionState(agent, session, conversationId, applicationId, userId);

            _agent = agent;
            _agentSession = session;
            _conversationId = conversationId;

            return CreateSessionContext();
        }
        finally
        {
            _initializeGate.Release();
        }
    }








    public void Dispose()
    {
        _initializeGate.Dispose();
        GC.SuppressFinalize(this);
    }








    private ConversationSessionContext CreateSessionContext()
    {
        ConversationSessionContext ctx = new ConversationSessionContext(_agent!, _agentSession!, _conversationId!);
        ctx.Identity = _providerSessionState!.GetOrInitializeState(_agentSession!);
        return ctx;
    }








    /// <summary>
    ///     Initializes the session state for the provider with the specified parameters.
    /// </summary>
    /// <param name="agent">The AI agent associated with the session.</param>
    /// <param name="agentSession">The session instance to be initialized.</param>
    /// <param name="conversationId">The unique identifier for the conversation.</param>
    /// <param name="applicationId">The identifier of the application initiating the session.</param>
    /// <param name="userId">The identifier of the user associated with the session.</param>
    private void InitializeProviderSessionState(AIAgent agent, AgentSession agentSession, string conversationId, string applicationId, string userId)
    {
        var providerState = new ProviderSessionState<HistoryIdentity>(currentSession => new HistoryIdentity { AgentId = agent.Id, ApplicationId = applicationId, ConversationId = conversationId, UserId = userId }, applicationId);
        //Extra check to ensure not null. Provider state will fail silently if agentSession is null, so this is just a safeguard.
        ArgumentNullException.ThrowIfNull(agentSession);
        HistoryIdentity identity = providerState.GetOrInitializeState(agentSession);
        providerState.SaveState(agentSession, identity);
        _providerSessionState = providerState;

    }








    private bool IsInitialized()
    {
        return _agent is not null && _agentSession is not null && !string.IsNullOrWhiteSpace(_conversationId);
    }








    internal string ResolveApplicationId()
    {
        var applicationId = _appSettings.ApplicationId?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(applicationId) ? "unknown-application" : applicationId;
    }








    /// <summary>
    ///     Resolves the startup conversation ID for a given agent, application, and user.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="applicationId">The unique identifier of the application.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> representing the asynchronous operation.
    ///     The result contains the resolved conversation ID.
    /// </returns>
    /// <remarks>
    ///     This method attempts to retrieve the latest conversation ID from the SQL chat history provider if available.
    ///     If no valid conversation ID is found, it falls back to the configured conversation ID in the application settings.
    ///     If neither is available, a new GUID is generated as the conversation ID.
    /// </remarks>
    internal async ValueTask<string> ResolveStartupConversationIdAsync(string agentId, string applicationId, string userId, CancellationToken cancellationToken)
    {
        if (_sqlChatHistoryProvider is not null)
        {
            var latestConversationId = await _sqlChatHistoryProvider.GetLatestConversationIdAsync(agentId, userId, applicationId, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(latestConversationId))
            {
                _appSettings.SetValue("LastConversationId", latestConversationId);
                return latestConversationId.Trim();
            }
        }

        var configuredConversationId = _appSettings.LastConversationId?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configuredConversationId))
        {
            return configuredConversationId;
        }

        var newConversationId = Guid.NewGuid().ToString("N");
        _appSettings.SetValue("LastConversationId", newConversationId);
        return newConversationId;
    }








    internal static string ResolveUserId()
    {
        var userId = Environment.UserName?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(userId) ? "unknown-user" : userId;
    }








    private async Task<bool> TryAcquireInitializationLockAsync(CancellationToken cancellationToken)
    {
        return await _initializeGate.WaitAsync(INITIALIZATION_LOCK_TIMEOUT, cancellationToken).ConfigureAwait(false);
    }
}