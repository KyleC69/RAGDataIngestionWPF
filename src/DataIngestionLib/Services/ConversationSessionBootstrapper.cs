using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





public sealed class ConversationSessionBootstrapper : IConversationSessionBootstrapper, IDisposable
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAgentIdentityProvider _agentIdentityProvider;
    private readonly IAppSettings _appSettings;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly ISQLChatHistoryProvider? _sqlChatHistoryProvider;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private string? _conversationId;





    public ConversationSessionBootstrapper(
            IAgentFactory agentFactory,
            IAppSettings appSettings,
            IAgentIdentityProvider agentIdentityProvider,
            ISQLChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(agentIdentityProvider);

        _agentFactory = agentFactory;
        _appSettings = appSettings;
        _agentIdentityProvider = agentIdentityProvider;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
    }





    public async ValueTask<ConversationSessionContext> EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_agent is not null && _agentSession is not null && !string.IsNullOrWhiteSpace(_conversationId))
        {
            return new ConversationSessionContext(_agent, _agentSession, _conversationId);
        }

        await _initializeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_agent is not null && _agentSession is not null && !string.IsNullOrWhiteSpace(_conversationId))
            {
                return new ConversationSessionContext(_agent, _agentSession, _conversationId);
            }

            string agentId = _agentIdentityProvider.GetAgentId();
            _agent = _agentFactory.GetCodingAssistantAgent(agentId, AIModels.GPTOSS, "Agentic-Max Description");

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ApplicationId", ResolveApplicationId());
            _agentSession.StateBag.SetValue("UserId", ResolveUserId());
            _agentSession.StateBag.SetValue("AgentId", agentId);

            _conversationId = await ResolveStartupConversationIdAsync(agentId, cancellationToken).ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ConversationId", _conversationId);

            return new ConversationSessionContext(_agent, _agentSession, _conversationId);
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





    private string ResolveApplicationId()
    {
        string applicationId = _appSettings.ApplicationId?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(applicationId) ? "unknown-application" : applicationId;
    }





    private static string ResolveUserId()
    {
        string userId = Environment.UserName?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(userId) ? "unknown-user" : userId;
    }





    private async ValueTask<string> ResolveStartupConversationIdAsync(string agentId, CancellationToken cancellationToken)
    {
        if (_sqlChatHistoryProvider is not null)
        {
            string? latestConversationId = await _sqlChatHistoryProvider
                    .GetLatestConversationIdAsync(agentId, ResolveUserId(), ResolveApplicationId(), cancellationToken)
                    .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(latestConversationId))
            {
                return latestConversationId.Trim();
            }
        }

        string configuredConversationId = _appSettings.LastConversationId?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configuredConversationId))
        {
            return configuredConversationId;
        }

        return Guid.NewGuid().ToString("N");
    }
}