// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatConversationService.cs
//   Author: Kyle L. Crowder



using System.IO;
using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Options;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatMessage = DataIngestionLib.Models.ChatMessage;




namespace DataIngestionLib.Services;





public sealed class ChatConversationService : IChatConversationService
{
    private readonly string _localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly ChatSessionOptions _options;
    private readonly ISqlVectorStore _sqlVectorStore;
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };








    public ChatConversationService(ChatSessionOptions options, IChatClient client, ILoggerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(factory);

        if (string.IsNullOrWhiteSpace(options.ConfigurationsFolder))
        {
            throw new ArgumentException("Configurations folder must be configured.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ChatSessionFileName))
        {
            throw new ArgumentException("Chat session file name must be configured.", nameof(options));
        }

        if (options.MaxContextTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum context tokens must be a positive value.");
        }








    }








    /// <summary>
    ///     Loads the persisted chat session for the current local user profile.
    /// </summary>
    /// <returns>The loaded chat session or a new empty session when no persisted state is available.</returns>
    public ChatSessionState LoadSession()
    {
        string path = GetSessionFilePath();
        if (!File.Exists(path))
        {
            return new();
        }

        string json = File.ReadAllText(path);
        ChatSessionState? state = JsonSerializer.Deserialize<ChatSessionState>(json, SerializerOptions);
        ChatSessionState normalizedState = state ?? new ChatSessionState();

        return NormalizeState(normalizedState);
    }








    /// <summary>
    ///     Persists the provided chat session state to local storage.
    /// </summary>
    /// <param name="sessionState">The session state to persist.</param>
    public void SaveSession(ChatSessionState sessionState)
    {
        ArgumentNullException.ThrowIfNull(sessionState);

        ChatSessionState normalizedState = NormalizeState(sessionState);
        string folder = GetStorageFolderPath();
        Directory.CreateDirectory(folder);

        string json = JsonSerializer.Serialize(normalizedState, SerializerOptions);
        File.WriteAllText(GetSessionFilePath(), json);
    }








    /// <summary>
    ///     Creates a user chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The raw user input content.</param>
    /// <returns>A normalized user chat message.</returns>
    public ChatMessage CreateUserMessage(string content)
    {
        return CreateMessage(ChatMessageRole.User, content);
    }








    /// <summary>
    ///     Creates an assistant chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The assistant response content.</param>
    /// <returns>A normalized assistant chat message.</returns>
    public ChatMessage CreateAssistantMessage(string content)
    {
        return CreateMessage(ChatMessageRole.Assistant, content);
    }








    /// <summary>
    ///     Generates an assistant response message asynchronously for the supplied user message.
    /// </summary>
    /// <param name="userMessage">The user message content to answer.</param>
    /// <param name="contextTokenCount">The active context token count at generation time.</param>
    /// <param name="cancellationToken">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    public async Task<ChatMessage> GenerateAssistantMessageAsync(string userMessage, int contextTokenCount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("User message cannot be empty.", nameof(userMessage));
        }



        /*

                var b = new AIAgentBuilder(_innerclient.AsAIAgent())
                        .UseLogging(_factory)
                        .UseAIContextProviders(new TextSearchProvider(), new ChatHistoryMemoryProvider(vectorStore), new MessageAIContextProvider())
                        .Build();


                // The AI agent is configured to use the following providers for context:
                // - TextSearchProvider: Allows the agent to search for text.
                // - ChatHistoryMemoryProvider: Provides access to the chat history.
                // - MessageAIContextProvider: Provides access to the current message.
                // The agent is also configured to use a tool invocation mechanism.
                // The _client2 represents the actual AI model that will be used for generation.
                // This setup is likely part of a more complex agent orchestration where
                // _innerclient might be a builder or a different type of client.
        */



        return CreateAssistantMessage($"Echo: {userMessage}");
    }








    /// <summary>
    ///     Appends a chat message into history and context while enforcing the configured sliding token window.
    /// </summary>
    /// <param name="sessionState">The current session state.</param>
    /// <param name="message">The message to append.</param>
    /// <returns>The updated session state after enforcing sliding context rules.</returns>
    public ChatSessionState AppendMessage(ChatSessionState sessionState, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(sessionState);
        ArgumentNullException.ThrowIfNull(message);

        ChatMessage normalizedMessage = NormalizeMessage(message);
        List<ChatMessage> history = sessionState.History.ToList();
        history.Add(normalizedMessage);

        List<ChatMessage> contextWindow = sessionState.ContextWindow.ToList();
        contextWindow.Add(normalizedMessage);

        int contextTokenCount = contextWindow.Sum(item => item.TokenCount);
        while (contextTokenCount > _options.MaxContextTokens && contextWindow.Count > 0)
        {
            contextTokenCount -= contextWindow[0].TokenCount;
            contextWindow.RemoveAt(0);
        }

        return new()
        {
            History = history,
            ContextWindow = contextWindow,
            ContextTokenCount = contextTokenCount
        };
    }








    private ChatSessionState AppendContextWindowLimit(ChatSessionState sessionState)
    {
        List<ChatMessage> contextWindow = sessionState.ContextWindow.ToList();
        int contextTokenCount = contextWindow.Sum(item => item.TokenCount);

        while (contextTokenCount > _options.MaxContextTokens && contextWindow.Count > 0)
        {
            contextTokenCount -= contextWindow[0].TokenCount;
            contextWindow.RemoveAt(0);
        }

        return sessionState with
        {
            ContextWindow = contextWindow,
            ContextTokenCount = contextTokenCount
        };
    }








    private ChatMessage CreateMessage(ChatMessageRole role, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Chat message content cannot be empty.", nameof(content));
        }

        string normalizedContent = content.Trim();
        return new()
        {
            Role = role,
            Content = normalizedContent,
            FormattedContent = FormatMarkdownLite(normalizedContent),
            TokenCount = EstimateTokenCount(normalizedContent),
            TimestampUtc = DateTimeOffset.UtcNow
        };
    }








    private static int EstimateTokenCount(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);
    }

    private Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IEnumerable<TextSearchProvider.TextSearchResult>>([]);
        }

        ChatSessionState sessionState = LoadSession();
        string trimmedQuery = query.Trim();

        IEnumerable<TextSearchProvider.TextSearchResult> results = sessionState.ContextWindow
                .Concat(sessionState.History)
                .Where(message => !string.IsNullOrWhiteSpace(message.Content))
                .OrderByDescending(message => message.TimestampUtc)
                .DistinctBy(message => message.TimestampUtc)
                .Where(message => message.Content.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .Select(message => new TextSearchProvider.TextSearchResult
                {
                    SourceName = "Local chat memory",
                    SourceLink = GetSessionFilePath(),
                    Text = message.Content
                });

        return Task.FromResult(results);
    }

    private static string FormatMarkdownLite(string content)
    {
        string normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("**", string.Empty, StringComparison.Ordinal)
                .Replace("__", string.Empty, StringComparison.Ordinal)
                .Replace("`", string.Empty, StringComparison.Ordinal);

        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                lines[i] = line[4..];
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                lines[i] = line[3..];
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                lines[i] = line[2..].ToUpperInvariant();
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
            {
                lines[i] = $"• {line[2..]}";
            }
        }

        return string.Join(Environment.NewLine, lines);
    }








    private string GetSessionFilePath()
    {
        return Path.Combine(GetStorageFolderPath(), _options.ChatSessionFileName);
    }

    private string GetStorageFolderPath()
    {
        return Path.Combine(_localAppDataPath, _options.ConfigurationsFolder);
    }

    private static ChatMessage NormalizeMessage(ChatMessage message)
    {
        string normalizedContent = message.Content.Trim() ?? string.Empty;
        int tokenCount = message.TokenCount <= 0 ? EstimateTokenCount(normalizedContent) : message.TokenCount;
        string formattedContent = string.IsNullOrWhiteSpace(message.FormattedContent)
                ? FormatMarkdownLite(normalizedContent)
                : message.FormattedContent;

        return message with
        {
            Content = normalizedContent,
            FormattedContent = formattedContent,
            TokenCount = tokenCount,
            TimestampUtc = message.TimestampUtc == default ? DateTimeOffset.UtcNow : message.TimestampUtc
        };
    }








    private ChatSessionState NormalizeState(ChatSessionState sessionState)
    {
        List<ChatMessage> normalizedHistory = (sessionState.History ?? []).Select(NormalizeMessage).ToList();
        List<ChatMessage> normalizedContext = (sessionState.ContextWindow ?? []).Select(NormalizeMessage).ToList();

        if (normalizedContext.Count == 0)
        {
            ChatSessionState rebuiltState = new();
            foreach (ChatMessage message in normalizedHistory)
            {
                rebuiltState = AppendMessage(rebuiltState, message);
            }

            return rebuiltState with { History = normalizedHistory };
        }

        return AppendContextWindowLimit(new()
        {
            History = normalizedHistory,
            ContextWindow = normalizedContext,
            ContextTokenCount = normalizedContext.Sum(message => message.TokenCount)
        });
    }
}