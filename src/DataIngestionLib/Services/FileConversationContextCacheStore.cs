// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FileConversationContextCacheStore.cs
// Author: Kyle L. Crowder
// Build Num: 140826



using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





/// <summary>
///     Stores cacheable context and tool messages per conversation as JSON for later retrieval.
/// </summary>
public sealed class FileConversationContextCacheStore : IConversationContextCacheStore
{

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _conversationLocks = [];
    private readonly string _rootDirectory;

    private static readonly HashSet<string> CacheableRoles = new(StringComparer.OrdinalIgnoreCase) { AIChatRole.AIContext.Value, AIChatRole.RAGContext.Value, AIChatRole.Tool.Value };

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
            "the",
            "and",
            "for",
            "with",
            "that",
            "this",
            "from",
            "into",
            "about",
            "what",
            "when",
            "where",
            "which",
            "while",
            "have",
            "does",
            "how",
            "why",
            "can",
            "should"
    };








    public FileConversationContextCacheStore(IAppSettings appSettings, string? rootDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        var applicationId = appSettings.ApplicationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            applicationId = "RAGDataIngestionWPF";
        }

        _rootDirectory = rootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationId, "conversation-context-cache");
    }








    public async ValueTask AppendAsync(string conversationId, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(messages);

        var normalizedConversationId = NormalizeConversationId(conversationId);
        if (normalizedConversationId.Length == 0 || messages.Count == 0)
        {
            return;
        }

        SemaphoreSlim gate = _conversationLocks.GetOrAdd(normalizedConversationId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var existingEntries = await LoadEntriesAsync(normalizedConversationId, cancellationToken).ConfigureAwait(false);
            var seen = existingEntries.Select(static entry => CreateDedupKey(entry.Role, entry.Text)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (ChatMessage message in messages)
            {
                var text = message.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var role = message.Role.Value?.Trim() ?? string.Empty;
                if (!IsCacheableRole(role))
                {
                    continue;
                }

                if (!seen.Add(CreateDedupKey(role, text)))
                {
                    continue;
                }

                existingEntries.Add(new ConversationContextCacheEntry
                {
                        EntryId = Guid.NewGuid(),
                        Role = role,
                        Text = text,
                        CreatedAtUtc = message.CreatedAt ?? DateTimeOffset.Now,
                        Keywords = Tokenize(text)
                });
            }

            await SaveEntriesAsync(normalizedConversationId, existingEntries, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }








    public async ValueTask ResetAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedConversationId = NormalizeConversationId(conversationId);
        if (normalizedConversationId.Length == 0)
        {
            return;
        }

        SemaphoreSlim gate = _conversationLocks.GetOrAdd(normalizedConversationId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var filePath = GetFilePath(normalizedConversationId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            gate.Release();
        }
    }








    public async ValueTask<IReadOnlyList<ConversationContextCacheEntry>> SearchAsync(string conversationId, string query, int maxResults, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedConversationId = NormalizeConversationId(conversationId);
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (normalizedConversationId.Length == 0 || string.IsNullOrWhiteSpace(normalizedQuery) || maxResults <= 0)
        {
            return [];
        }

        SemaphoreSlim gate = _conversationLocks.GetOrAdd(normalizedConversationId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IReadOnlyList<ConversationContextCacheEntry> entries = await LoadEntriesAsync(normalizedConversationId, cancellationToken).ConfigureAwait(false);
            if (entries.Count == 0)
            {
                return [];
            }

            var queryTerms = Tokenize(normalizedQuery);
            return entries.Select(entry => (Entry: entry, Score: Score(entry, normalizedQuery, queryTerms))).Where(static pair => pair.Score > 0).OrderByDescending(pair => pair.Score).ThenByDescending(pair => pair.Entry.CreatedAtUtc).Take(maxResults).Select(pair => pair.Entry).ToArray();
        }
        finally
        {
            gate.Release();
        }
    }








    internal static string CreateDedupKey(string role, string text)
    {
        return role.Trim() + "\n" + text.Trim();
    }








    internal string GetFilePath(string conversationId)
    {
        return Path.Combine(_rootDirectory, conversationId + ".json");
    }








    internal static bool IsCacheableRole(string role)
    {
        var normalizedRole = role.Trim();
        return CacheableRoles.Contains(normalizedRole) || normalizedRole.EndsWith("_context", StringComparison.OrdinalIgnoreCase);
    }








    internal async ValueTask<List<ConversationContextCacheEntry>> LoadEntriesAsync(string conversationId, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(conversationId);
        if (!File.Exists(filePath))
        {
            return [];
        }

        FileStream stream = File.OpenRead(filePath);
        await using ConfiguredAsyncDisposable stream1 = stream.ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<List<ConversationContextCacheEntry>>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false) ?? [];
    }








    internal static string NormalizeConversationId(string conversationId)
    {
        var trimmed = conversationId?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var invalid = Path.GetInvalidFileNameChars();
        return new string(trimmed.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }








    internal async ValueTask SaveEntriesAsync(string conversationId, IReadOnlyList<ConversationContextCacheEntry> entries, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(_rootDirectory);
        var filePath = GetFilePath(conversationId);

        FileStream stream = File.Create(filePath);
        await using ConfiguredAsyncDisposable stream1 = stream.ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, entries, SerializerOptions, cancellationToken).ConfigureAwait(false);
    }








    internal static double Score(ConversationContextCacheEntry entry, string query, IReadOnlyList<string> queryTerms)
    {
        var text = entry.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return 0;
        }

        double score = 0;
        if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        if (entry.Keywords.Any(keyword => queryTerms.Contains(keyword, StringComparer.OrdinalIgnoreCase)))
        {
            score += entry.Keywords.Count(keyword => queryTerms.Contains(keyword, StringComparer.OrdinalIgnoreCase));
        }

        return score;
    }








    internal static string[] Tokenize(string text)
    {
        return text.Split([
                        ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '/', '\\', '"', '\''
                ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(term => term.Length >= 3 && !StopWords.Contains(term))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}