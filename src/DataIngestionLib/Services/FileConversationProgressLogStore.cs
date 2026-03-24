// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FileConversationProgressLogStore.cs
// Author: Kyle L. Crowder
// Build Num: 140827



using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;




namespace DataIngestionLib.Services;





public sealed class FileConversationProgressLogStore : IConversationProgressLogStore
{

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _conversationLocks = [];
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);








    public FileConversationProgressLogStore(IAppSettings appSettings, string? rootDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        var applicationId = appSettings.ApplicationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            applicationId = "RAGDataIngestionWPF";
        }

        _rootDirectory = rootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "conversation-progress-logs");
    }








    public async ValueTask DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default)
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








    public async ValueTask<ConversationProgressLog?> GetAsync(string conversationId, Guid planId, CancellationToken cancellationToken = default)
    {
        if (planId == Guid.Empty)
        {
            return null;
        }

        var plans = await ListAsync(conversationId, cancellationToken).ConfigureAwait(false);
        return plans.FirstOrDefault(plan => plan.PlanId == planId);
    }








    public async ValueTask<IReadOnlyList<ConversationProgressLog>> ListAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedConversationId = NormalizeConversationId(conversationId);
        if (normalizedConversationId.Length == 0)
        {
            return [];
        }

        SemaphoreSlim gate = _conversationLocks.GetOrAdd(normalizedConversationId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await LoadPlansAsync(normalizedConversationId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }








    public async ValueTask SaveAsync(ConversationProgressLog progressLog, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(progressLog);

        var normalizedConversationId = NormalizeConversationId(progressLog.ConversationId);
        if (normalizedConversationId.Length == 0)
        {
            throw new ArgumentException("Conversation ID is required.", nameof(progressLog));
        }

        SemaphoreSlim gate = _conversationLocks.GetOrAdd(normalizedConversationId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var plans = await LoadPlansAsync(normalizedConversationId, cancellationToken).ConfigureAwait(false);
            var existingIndex = plans.FindIndex(plan => plan.PlanId == progressLog.PlanId);
            ConversationProgressLog normalizedPlan = progressLog with { ConversationId = normalizedConversationId, UpdatedAtUtc = progressLog.UpdatedAtUtc == default ? DateTimeOffset.Now : progressLog.UpdatedAtUtc };

            if (existingIndex >= 0)
            {
                plans[existingIndex] = normalizedPlan;
            }
            else
            {
                plans.Add(normalizedPlan);
            }

            plans.Sort(static (left, right) => right.UpdatedAtUtc.CompareTo(left.UpdatedAtUtc));
            await SavePlansAsync(normalizedConversationId, plans, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }








    internal string GetFilePath(string conversationId)
    {
        return Path.Combine(_rootDirectory, conversationId + ".json");
    }








    internal async ValueTask<List<ConversationProgressLog>> LoadPlansAsync(string conversationId, CancellationToken cancellationToken)
    {
        var filePath = GetFilePath(conversationId);
        if (!File.Exists(filePath))
        {
            return [];
        }

        FileStream stream = File.OpenRead(filePath);
        await using ConfiguredAsyncDisposable stream1 = stream.ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<List<ConversationProgressLog>>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false) ?? [];
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








    internal async ValueTask SavePlansAsync(string conversationId, IReadOnlyList<ConversationProgressLog> plans, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(_rootDirectory);
        var filePath = GetFilePath(conversationId);

        FileStream stream = File.Create(filePath);
        await using ConfiguredAsyncDisposable stream1 = stream.ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, plans, SerializerOptions, cancellationToken).ConfigureAwait(false);
    }
}