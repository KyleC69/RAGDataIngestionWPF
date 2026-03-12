using System.Reflection;

using DataIngestionLib.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

using RAGDataIngestionWPF.Contracts.Services;

namespace RAGDataIngestionWPF.Services;

/// <summary>
///     Stores chat history settings in the current-user registry under an AppData-scoped key
///     and exposes runtime updates through <see cref="IOptionsMonitor{TOptions}" />.
/// </summary>
public sealed class ChatHistorySettingsService : IChatHistorySettingsService, IOptionsMonitor<ChatHistoryOptions>
{
    private const string ChatHistoryContextEnabledValueName = "ChatHistoryContextEnabled";
    private const string ChatModelNameValueName = "ChatModelName";
    private const string ConnectionStringValueName = "ConnectionString";
    private const string EmbeddingsModelNameValueName = "EmbeddingsModelName";
    private const string MaxContextMessagesValueName = "MaxContextMessages";
    private const string MaxContextTokensValueName = "MaxContextTokens";
    private const string RagKnowledgeEnabledValueName = "RagKnowledgeEnabled";

    private readonly List<Action<ChatHistoryOptions, string?>> _listeners = [];
    private readonly Lock _syncRoot = new();
    private readonly string _registryPath;

    private ChatHistoryOptions _currentSettings;

    public ChatHistorySettingsService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _registryPath = BuildRegistryPath();
        ChatHistoryOptions defaults = configuration
                .GetSection(ChatHistoryOptions.ConfigurationSectionName)
                .Get<ChatHistoryOptions>()
                ?? new ChatHistoryOptions();

        _currentSettings = ReadSettingsFromRegistry(defaults);
        WriteSettingsToRegistry(_currentSettings);
    }

    public ChatHistoryOptions CurrentValue => GetCurrentSettings();

    public ChatHistoryOptions Get(string? name)
    {
        return GetCurrentSettings();
    }

    public ChatHistoryOptions GetCurrentSettings()
    {
        lock (_syncRoot)
        {
            return Clone(_currentSettings);
        }
    }

    public IDisposable OnChange(Action<ChatHistoryOptions, string?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        lock (_syncRoot)
        {
            _listeners.Add(listener);
        }

        return new Subscription(this, listener);
    }

    public void SaveSettings(ChatHistoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ChatHistoryOptions validated = ValidateAndNormalize(options);
        List<Action<ChatHistoryOptions, string?>> listeners;

        lock (_syncRoot)
        {
            _currentSettings = validated;
            WriteSettingsToRegistry(validated);
            listeners = [.. _listeners];
        }

        foreach (Action<ChatHistoryOptions, string?> listener in listeners)
        {
            listener(Clone(validated), null);
        }
    }

    private static string BuildRegistryPath()
    {
        Assembly entryAssembly = Assembly.GetEntryAssembly() ?? typeof(ChatHistorySettingsService).Assembly;
        string productName = entryAssembly.GetName().Name ?? "RAGDataIngestionWPF";
        string? companyName = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        return string.IsNullOrWhiteSpace(companyName)
                ? $@"Software\\{productName}\\AppData\\ChatHistory"
                : $@"Software\\{companyName}\\{productName}\\AppData\\ChatHistory";
    }

    private static ChatHistoryOptions Clone(ChatHistoryOptions options)
    {
        return new ChatHistoryOptions
        {
            ChatModelName = options.ChatModelName,
            ConnectionString = options.ConnectionString,
            EmbeddingsModelName = options.EmbeddingsModelName,
            MaxContextMessages = options.MaxContextMessages,
            MaxContextTokens = options.MaxContextTokens,
            RAGKnowledgeEnabled = options.RAGKnowledgeEnabled,
            ChatHistoryContextEnabled = options.ChatHistoryContextEnabled
        };
    }

    private static int? ParseNullableInt(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        string raw = value.ToString() ?? string.Empty;
        return int.TryParse(raw, out int parsed) ? parsed : null;
    }

    private static bool ParseRegistryBoolean(object? value, bool fallback)
    {
        if (value is null)
        {
            return fallback;
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        string raw = value.ToString() ?? string.Empty;
        if (bool.TryParse(raw, out bool parsedBool))
        {
            return parsedBool;
        }

        return int.TryParse(raw, out int parsedInt) ? parsedInt != 0 : fallback;
    }

    private static ChatHistoryOptions ValidateAndNormalize(ChatHistoryOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ChatModelName))
        {
            throw new ArgumentException("Chat model name is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.EmbeddingsModelName))
        {
            throw new ArgumentException("Embeddings model name is required.", nameof(options));
        }

        if (options.MaxContextMessages < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Max context messages must be greater than zero.");
        }

        if (options.MaxContextTokens is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Max context tokens must be greater than zero when provided.");
        }

        return new ChatHistoryOptions
        {
            ChatModelName = options.ChatModelName.Trim(),
            ConnectionString = options.ConnectionString.Trim(),
            EmbeddingsModelName = options.EmbeddingsModelName.Trim(),
            MaxContextMessages = options.MaxContextMessages,
            MaxContextTokens = options.MaxContextTokens,
            RAGKnowledgeEnabled = options.RAGKnowledgeEnabled,
            ChatHistoryContextEnabled = options.ChatHistoryContextEnabled
        };
    }

    private RegistryKey OpenRegistryKey()
    {
        return Registry.CurrentUser.CreateSubKey(_registryPath, writable: true)
               ?? throw new InvalidOperationException($"Unable to open registry path '{_registryPath}'.");
    }

    private ChatHistoryOptions ReadSettingsFromRegistry(ChatHistoryOptions defaults)
    {
        using RegistryKey registryKey = OpenRegistryKey();

        int maxContextMessages = ParseNullableInt(registryKey.GetValue(MaxContextMessagesValueName)) ?? defaults.MaxContextMessages;
        int? maxContextTokens = ParseNullableInt(registryKey.GetValue(MaxContextTokensValueName)) ?? defaults.MaxContextTokens;

        ChatHistoryOptions options = new()
        {
            ChatModelName = registryKey.GetValue(ChatModelNameValueName)?.ToString() ?? defaults.ChatModelName,
            ConnectionString = registryKey.GetValue(ConnectionStringValueName)?.ToString() ?? defaults.ConnectionString,
            EmbeddingsModelName = registryKey.GetValue(EmbeddingsModelNameValueName)?.ToString() ?? defaults.EmbeddingsModelName,
            MaxContextMessages = maxContextMessages,
            MaxContextTokens = maxContextTokens,
            RAGKnowledgeEnabled = ParseRegistryBoolean(registryKey.GetValue(RagKnowledgeEnabledValueName), defaults.RAGKnowledgeEnabled),
            ChatHistoryContextEnabled = ParseRegistryBoolean(registryKey.GetValue(ChatHistoryContextEnabledValueName), defaults.ChatHistoryContextEnabled)
        };

        return ValidateAndNormalize(options);
    }

    private void RemoveListener(Action<ChatHistoryOptions, string?> listener)
    {
        lock (_syncRoot)
        {
            _listeners.Remove(listener);
        }
    }

    private void WriteSettingsToRegistry(ChatHistoryOptions options)
    {
        using RegistryKey registryKey = OpenRegistryKey();
        registryKey.SetValue(ChatModelNameValueName, options.ChatModelName, RegistryValueKind.String);
        registryKey.SetValue(ConnectionStringValueName, options.ConnectionString, RegistryValueKind.String);
        registryKey.SetValue(EmbeddingsModelNameValueName, options.EmbeddingsModelName, RegistryValueKind.String);
        registryKey.SetValue(MaxContextMessagesValueName, options.MaxContextMessages, RegistryValueKind.DWord);

        if (options.MaxContextTokens.HasValue)
        {
            registryKey.SetValue(MaxContextTokensValueName, options.MaxContextTokens.Value, RegistryValueKind.DWord);
        }
        else
        {
            registryKey.DeleteValue(MaxContextTokensValueName, throwOnMissingValue: false);
        }

        registryKey.SetValue(RagKnowledgeEnabledValueName, options.RAGKnowledgeEnabled ? 1 : 0, RegistryValueKind.DWord);
        registryKey.SetValue(ChatHistoryContextEnabledValueName, options.ChatHistoryContextEnabled ? 1 : 0, RegistryValueKind.DWord);
    }

    private sealed class Subscription(ChatHistorySettingsService owner, Action<ChatHistoryOptions, string?> listener) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            owner.RemoveListener(listener);
            _isDisposed = true;
        }
    }
}
