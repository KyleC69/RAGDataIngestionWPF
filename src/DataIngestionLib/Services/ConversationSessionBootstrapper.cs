// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationSessionBootstrapper.cs
// Author: Kyle L. Crowder
// Build Num: 160439



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;




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
            return new ConversationSessionContext(_agent!, _agentSession!, _conversationId!);
        }

        if (!await _initializeGate.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false))
        {
            throw new TimeoutException("Failed to acquire initialization lock within the timeout period.");
        }

        try
        {
            if (IsInitialized())
            {
                return new ConversationSessionContext(_agent!, _agentSession!, _conversationId!);
            }

            _agent = _agentFactory.GetCodingAssistantAgent("Agentic-Max", AIModels.GPTOSS, "Agentic-Max Description");
            if (_appSettings == null)
            {
                throw new InvalidOperationException("AppSettings is not initialized.");
            }

            _conversationId = await ResolveStartupConversationIdAsync(_agent.Id, ResolveApplicationId(), ResolveUserId(), cancellationToken);
            _appSettings.SetValue("LastConversationId", _conversationId);
            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ApplicationId", ResolveApplicationId());
            _agentSession.StateBag.SetValue("UserId", ResolveUserId());
            _agentSession.StateBag.SetValue("AgentId", _agent.Id);
            _agentSession.StateBag.SetValue("ConversationId", _conversationId);
            return new ConversationSessionContext(_agent, _agentSession, _conversationId);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.Error.WriteLine($"Error during initialization: {ex.Message}");
            throw;
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








    private bool IsInitialized()
    {
        return _agent is not null && _agentSession is not null && !string.IsNullOrWhiteSpace(_conversationId);
    }








    private string LoadLastConversationIdFromDB()
    {
        AIChatHistoryDb db = new AIChatHistoryDb();
        var response = db.Database.ExecuteSqlInterpolated($"EXEC sp_GetLastConversationId({ResolveApplicationId()}, {ResolveUserId()}, {_agent.Id}  )");
        return response.ToString();
    }








    internal string ResolveApplicationId()
    {
        var applicationId = _appSettings.ApplicationId?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(applicationId) ? "unknown-application" : applicationId;
    }








    internal async ValueTask<string> ResolveStartupConversationIdAsync(string agentId, string ApplicationId,string UserId,CancellationToken cancellationToken)
    {
        if (_sqlChatHistoryProvider is not null)
        {
            var latestConversationId = await _sqlChatHistoryProvider.GetLatestConversationIdAsync(agentId, UserId, ApplicationId, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(latestConversationId))
            {
                return latestConversationId.Trim();
            }
        }

        var configuredConversationId = _appSettings.LastConversationId?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configuredConversationId))
        {
            return configuredConversationId;
        }

        return Guid.NewGuid().ToString("N");
    }








    internal static string ResolveUserId()
    {
        var userId = Environment.UserName?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(userId) ? "unknown-user" : userId;
    }
}