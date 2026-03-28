// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using System.Reflection;
using System.Text.Json;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.History.HistoryModels;
using DataIngestionLib.HistoryModels;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class SqlChatHistoryProvider : ChatHistoryProvider, ISQLChatHistoryProvider, IDisposable
{
    private readonly IAppSettings _appSettings;
    private readonly AIChatHistoryDb _dbContext;

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly ILogger<SqlChatHistoryProvider> _logger;

    private readonly ProviderSessionState<HistoryIdentity> _sessionState;

    private static readonly HashSet<AgentRequestMessageSourceType> IgnoredRequestSourceTypes =
    [
            AgentRequestMessageSourceType.ChatHistory
    ];








    public SqlChatHistoryProvider(ILogger<SqlChatHistoryProvider> logger, IAppSettings appSettings, AIChatHistoryDb? dbContext = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);

        _logger = logger;
        _appSettings = appSettings;
        _dbContext = dbContext ?? new AIChatHistoryDb();

        var sessionStateKey = string.IsNullOrWhiteSpace(_appSettings.ApplicationId) ? nameof(HistoryIdentity) : _appSettings.ApplicationId;
        _sessionState = new ProviderSessionState<HistoryIdentity>(_ => new HistoryIdentity { AgentId = "", ApplicationId = "", ConversationId = "", UserId = "" }, sessionStateKey);

        if (_appSettings.ResumeLast)
        {
            this.ResumeConversation();
        }
    }








    /// <summary>
    ///     When overridden in a derived class, provides the chat history messages to be used for the current invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is called from
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         .
    ///         Note that
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         can be overridden to directly control message filtering, merging and source stamping, in which case
    ///         it is up to the implementer to call this method as needed to retrieve the unfiltered/unmerged chat history
    ///         messages.
    ///     </para>
    ///     <para>
    ///         In contrast with
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         , this method only returns additional messages to be added to the request,
    ///         while
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         is responsible for returning the full set of messages to be used for the invocation (including caller provided
    ///         messages).
    ///     </para>
    ///     <para>
    ///         Messages are returned in chronological order to maintain proper conversation flow and context for the agent.
    ///         The oldest messages appear first in the collection, followed by more recent messages.
    ///     </para>
    ///     <para>
    ///         <strong>Security consideration:</strong> Messages loaded from storage should be treated with the same caution
    ///         as user-supplied
    ///         messages. A compromised storage backend could alter message roles to escalate trust (e.g., changing <c>user</c>
    ///         messages to
    ///         <c>system</c> messages) or inject adversarial content that influences LLM behavior.
    ///     </para>
    /// </remarks>
    /// <param name="context">
    ///     Contains the request context including the caller provided messages that will be used by the
    ///     agent for this invocation.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of
    ///     <see cref="T:Microsoft.Extensions.AI.ChatMessage" />
    ///     instances in ascending chronological order (oldest first).
    /// </returns>
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        HistoryIdentity state = _sessionState.GetOrInitializeState(context.Session);
        ArgumentNullException.ThrowIfNull(state);


        var req = context.RequestMessages.FirstOrDefault(m => m.Role == ChatRole.User)?.Text;
        //Search previous chat history based on the current user message and return relevant messages.
        //This is a lightweight enhancement, but one of the most important to giving the end user a very
        //human like conversation experience. The agent appears to to remember previous topics of conversation.
        // The seach pattern here is a multi-stage broad net to a semantically fine-tuned and ranked result set.
        List<ChatHistoryMessage> res = string.IsNullOrWhiteSpace(req) ? [] : await this.SearchChatHistory(req).ConfigureAwait(false);


        IReadOnlyList<ChatMessage> msg = res.ToChatMessages();

        IReadOnlyList<PersistedChatMessage> historyMessages = await this.GetMessagesAsync(state, cancellationToken).ConfigureAwait(false);

        // Filter out messages that are already present in context.RequestMessages
        IEnumerable<PersistedChatMessage> uniqueHistoryMessages = historyMessages.Where(hm => !context.RequestMessages.Any(rm => rm.MessageId == hm.MessageId.ToString("D")));

        // Convert to ChatMessage and add source type
        List<ChatMessage> chatHistoryMessages = uniqueHistoryMessages.Select(hm =>
                {
                    ChatMessage chatMessage = hm.ToChatMessage();
                    // Use the MessageId of the persisted message as the source ID
                    return chatMessage.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, hm.MessageId.ToString("D"));
                })
                .ToList();

        // Add the search results to the history
        chatHistoryMessages.AddRange(msg);

        return chatHistoryMessages.OrderBy(m => m.CreatedAt ?? DateTimeOffset.MinValue);






    }








    /// <summary>
    ///     When overridden in a derived class, adds new messages to the chat history at the end of the agent invocation.
    /// </summary>
    /// <param name="context">
    ///     Contains the invocation context including request messages, response messages, and any exception
    ///     that occurred.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    /// <remarks>
    ///     <para>
    ///         Messages should be added in the order they were generated to maintain proper chronological sequence.
    ///         The <see cref="T:Microsoft.Agents.AI.ChatHistoryProvider" /> is responsible for preserving message ordering and
    ///         ensuring that subsequent calls to
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         return messages in the correct chronological order.
    ///     </para>
    ///     <para>
    ///         Implementations may perform additional processing during message addition, such as:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>Validating message content and metadata</description>
    ///             </item>
    ///             <item>
    ///                 <description>Applying storage optimizations or compression</description>
    ///             </item>
    ///             <item>
    ///                 <description>Triggering background maintenance operations</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This method is called from
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         .
    ///         Note that
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         can be overridden to directly control message filtering and error handling, in which case
    ///         it is up to the implementer to call this method as needed to store messages.
    ///     </para>
    ///     <para>
    ///         In contrast with
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         , this method only stores messages,
    ///         while
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         is also responsible for messages filtering and error handling.
    ///     </para>
    ///     <para>
    ///         The default implementation of
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         only calls this method if the invocation succeeded.
    ///     </para>
    ///     <para>
    ///         <strong>Security consideration:</strong> Messages being stored may contain PII and sensitive conversation
    ///         content.
    ///         Implementers should ensure appropriate encryption at rest and access controls for the storage backend.
    ///     </para>
    /// </remarks>
    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new())
    {
        //Method needs to save new messages from the current invocation to the database.
        //It should not re-save messages that were loaded from the database and included in the request messages for this invocation,
        //as that would create duplicates. It can identify which messages are new based on the presence of the AgentRequestMessageSourceType.ChatHistory source type
        //and the unique source ID assigned to messages loaded from history.
        try
        {
            HistoryIdentity state = _sessionState.GetOrInitializeState(context.Session);


            IReadOnlyList<ChatMessage> requestMessages = FilterRequestMessages(context.RequestMessages);
            IReadOnlyList<ChatMessage> responseMessages = GetContextMessages(context, "ResponseMessages");

            if (responseMessages.Count == 0)
            {
                responseMessages = GetContextMessages(context, "ResultMessages");
            }

            if (requestMessages.Count == 0 && responseMessages.Count == 0)
            {
                return;
            }

            await this.PersistInteractionAsync(state, requestMessages, responseMessages, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to persist invoked chat interaction. Continuing without failing the active agent run.");
        }
    }








    public void Dispose()
    {
        _initializationGate.Dispose();
    }








    public HistoryIdentity SessionState { get; set; } = new();








    /// <summary>
    ///     Asynchronously creates a new chat message in the SQL database.
    /// </summary>
    /// <param name="message">
    ///     The <see cref="PersistedChatMessage" /> instance representing the chat message to be created.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that represents the asynchronous operation.
    ///     The task result contains the created <see cref="PersistedChatMessage" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="message" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown if the operation is canceled via the <paramref name="cancellationToken" />.
    /// </exception>
    public async ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Begin CreateMessageAsync message role:{0}", message.Role);
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();


        ChatHistoryMessage entity = this.ToEntity(message);

        _ = await _dbContext.ChatHistoryMessages.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return this.ToPersisted(entity);
    }








    public async ValueTask<string?> GetLatestConversationIdAsync(string agentId, string userId, string applicationId, CancellationToken cancellationToken = default)
    {

        cancellationToken.ThrowIfCancellationRequested();




        var latest = await _dbContext.ChatHistoryMessages.AsNoTracking().Where(message => message.AgentId == agentId && message.UserId == userId && message.ApplicationId == applicationId && message.ConversationId != string.Empty).OrderByDescending(message => message.CreatedAt).Select(message => message.ConversationId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(latest) ? null : latest;
    }








    public async ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID must be a non-empty GUID.", nameof(messageId));
        }

        cancellationToken.ThrowIfCancellationRequested();


        ChatHistoryMessage? entity = await _dbContext.ChatHistoryMessages.AsNoTracking().FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken).ConfigureAwait(false);

        return entity is null ? null : this.ToPersisted(entity);
    }








    /// <summary>
    ///     Retrieves a list of persisted chat messages from the SQL database for a specific identity,
    ///     which includes conversation ID, application ID, and user ID.
    ///     The messages are returned in chronological order based on their timestamp.
    ///     These messages should be stamped with the meta data ChatHistory to allow the
    ///     StoreChatHistoryAsync method to identify them as messages loaded from history and avoid re-saving them, which would
    ///     create duplicates.
    /// </summary>
    /// <param name="identity">
    ///     The <see cref="HistoryIdentity" /> instance containing the identifiers for the conversation, application, and user.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of
    ///     <see cref="PersistedChatMessage" /> objects corresponding to the specified conversation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="identity" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown if the operation is canceled via the <paramref name="cancellationToken" />.
    /// </exception>
    public async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(HistoryIdentity identity, CancellationToken cancellationToken = default)
    {

        cancellationToken.ThrowIfCancellationRequested();


        IQueryable<ChatHistoryMessage> ordered = _dbContext.ChatHistoryMessages.AsNoTracking().Where(message => message.ConversationId == identity.ConversationId && message.ApplicationId == identity.ApplicationId && message.UserId == identity.UserId).OrderByDescending(message => message.CreatedAt);



        List<ChatHistoryMessage> entities = await ordered.OrderBy(message => message.TimestampUtc).ThenBy(message => message.CreatedAt).ToListAsync(cancellationToken).ConfigureAwait(false);



        List<PersistedChatMessage> messages = entities.ConvertAll(this.ToPersisted);

        return messages;
    }








    /// <summary>
    ///     Filters the provided collection of request messages by removing messages with ignored source types
    ///     or those with empty or whitespace-only text content.
    /// </summary>
    /// <param name="requestMessages">
    ///     A collection of <see cref="ChatMessage" /> objects to be filtered.
    ///     If <c>null</c>, an empty list is returned.
    /// </param>
    /// <returns>
    ///     A read-only list of <see cref="ChatMessage" /> objects that meet the filtering criteria.
    /// </returns>
    private static IReadOnlyList<ChatMessage> FilterRequestMessages(IEnumerable<ChatMessage>? requestMessages)
    {
        if (requestMessages is null)
        {
            return [];
        }



        List<ChatMessage> filteredMessages = requestMessages.Where(message => !IgnoredRequestSourceTypes.Contains(message.GetAgentRequestMessageSourceType()) && !string.IsNullOrWhiteSpace(message.Text)).ToList();


        return filteredMessages;

    }








    private static IReadOnlyList<ChatMessage> GetContextMessages(object context, string propertyName)
    {
        PropertyInfo? property = context.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(context) is IEnumerable<ChatMessage> messages ? messages.Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray() : (IReadOnlyList<ChatMessage>)[];
    }








    public async Task<ChatHistoryMessage?> GetLastMessageAsync()
    {
        ChatHistoryMessage? entity = _dbContext.ChatHistoryMessages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        return entity;
    }








    private static string NormalizeContent(string content, string parameterName)
    {
        return string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Message content cannot be empty.", parameterName) : content.Trim();
    }








    private JsonDocument? ParseMetadata(string? metadataJson, Guid messageId)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(metadataJson);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Invalid metadata JSON was found for chat history message {MessageId}. Metadata will be ignored.", messageId);
            return null;
        }
    }








    /// <summary>
    ///     Persists the interaction consisting of request and response messages to the SQL database. Messages are associated
    ///     with the provided history identity, which includes conversation ID, application ID, and user ID.
    /// </summary>
    /// <param name="state">The history identity containing conversation ID, application ID, and user ID.</param>
    /// <param name="requestMessages">The collection of request messages to be persisted.</param>
    /// <param name="responseMessages">The collection of response messages to be persisted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async ValueTask PersistInteractionAsync(HistoryIdentity state, IReadOnlyList<ChatMessage> requestMessages, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken)
    {

        List<ChatHistoryMessage> entities = [];
        entities.AddRange(this.ToEntities(requestMessages, state.ConversationId, state.AgentId, state.UserId, state.ApplicationId));
        entities.AddRange(this.ToEntities(responseMessages, state.ConversationId, state.AgentId, state.UserId, state.ApplicationId));

        if (entities.Count == 0)
        {
            return;
        }

        await _dbContext.ChatHistoryMessages.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }








    private void ResumeConversation()
    {
        //Load the most recent conversation for the agent and user from the database
        //and set it in the session state so that it can be used to load the correct chat history messages for this invocation.
        //This only needs to be called once at Application start.
    }








    private static string RoleToString(ChatRole role)
    {
        var value = role.Value.Trim();
        return value;
    }








    internal async Task<List<ChatHistoryMessage>> SearchChatHistory(string query)
    {
        HybridSearch hybrid = new();
        using AIChatHistoryDb db = new();
        hybrid.SearchPhrase = query;


        try
        {


            IQueryable<ChatHistoryMessage> results = db.ChatHistoryMessages.FromSqlInterpolated($"EXEC sp_Search_FreeText @Query={query}, @TopN=5");

            return results.ToList();

        }
        catch (Exception)
        {
            _logger.LogError("Error occurred while searching chat history.");
        }

        return new();

    }








    private string? SerializeMetadata(IReadOnlyDictionary<string, object?>? additionalProperties)
    {
        if (additionalProperties is null || additionalProperties.Count == 0)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(additionalProperties);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Chat message metadata could not be serialized and will be skipped.");
            return null;
        }
    }








    private List<ChatHistoryMessage> ToEntities(IEnumerable<ChatMessage> messages, string conversationId, string agentId, string userId, string applicationId)
    {

        _logger.LogTrace("beginning chat history persistance.");
        HashSet<Guid> seenMessageIds = [];
        List<ChatHistoryMessage> entities = [];

        try
        {
            foreach (ChatMessage message in messages)
            {
                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    continue;
                }

                Guid messageId = TryParseMessageId(message.MessageId);
                if (!seenMessageIds.Add(messageId))
                {
                    continue;
                }

                entities.Add(new ChatHistoryMessage
                {
                    MessageId = messageId,
                    ConversationId = conversationId,
                    AgentId = agentId,
                    UserId = userId,
                    ApplicationId = applicationId,
                    Role = RoleToString(message.Role),
                    Content = message.Text.Trim(),
                    TimestampUtc = message.CreatedAt ?? DateTimeOffset.Now,
                    Metadata = this.SerializeMetadata(message.AdditionalProperties),
                    CreatedAt = DateTime.Now,
                    Enabled = true
                });
            }
        }
        catch (DbUpdateException db)
        {

            _logger.LogError(db, "Error updating chat history messages.");
        }

        return entities;
    }








    private ChatHistoryMessage ToEntity(PersistedChatMessage message)
    {
        return new ChatHistoryMessage
        {
            MessageId = message.MessageId == Guid.Empty ? Guid.NewGuid() : message.MessageId,
            ConversationId = message.ConversationId,
            AgentId = message.AgentId,
            UserId = message.UserId,
            ApplicationId = message.ApplicationId,
            Role = message.Role,
            Content = message.Content,
            TimestampUtc = message.TimestampUtc == default ? DateTime.Now : message.TimestampUtc,
            Metadata = message.Metadata?.RootElement.GetRawText(),
            CreatedAt = DateTime.Now,
            Enabled = true
        };
    }








    private PersistedChatMessage ToPersisted(ChatHistoryMessage message)
    {
        return new PersistedChatMessage
        {
            MessageId = message.MessageId,
            ConversationId = message.ConversationId,
            AgentId = message.AgentId,
            UserId = message.UserId,
            ApplicationId = message.ApplicationId,
            Role = message.Role,
            Content = message.Content,
            TimestampUtc = message.TimestampUtc,
            Metadata = this.ParseMetadata(message.Metadata, message.MessageId)
        };
    }








    private static Guid TryParseMessageId(string? messageId)
    {
        return Guid.TryParse(messageId, out Guid parsedMessageId) ? parsedMessageId : Guid.NewGuid();
    }








    public async ValueTask<PersistedChatMessage?> UpdateMessageAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID must be a non-empty GUID.", nameof(messageId));
        }

        var normalizedContent = NormalizeContent(content, nameof(content));
        cancellationToken.ThrowIfCancellationRequested();

        ChatHistoryMessage? entity = await _dbContext.ChatHistoryMessages.FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        entity.Content = normalizedContent;
        entity.TimestampUtc = timestampUtc;

        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return this.ToPersisted(entity);
    }








    public class HybridSearch
    {
        public string SearchPhrase { get; set; } = string.Empty;

        public string VectorQuery { get; set; } = string.Empty;
    }
}