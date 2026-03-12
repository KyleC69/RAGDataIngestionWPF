using DataIngestionLib.Options;

namespace RAGDataIngestionWPF.Contracts.Services;

/// <summary>
///     Provides UI-facing read and write access to chat history options that are persisted
///     in the current user's registry hive.
/// </summary>
public interface IChatHistorySettingsService
{
    /// <summary>
    ///     Gets the current chat history settings snapshot used by the running application.
    /// </summary>
    ChatHistoryOptions GetCurrentSettings();

    /// <summary>
    ///     Persists chat history settings and updates the in-memory settings snapshot for the
    ///     active application process.
    /// </summary>
    /// <param name="options">The chat history settings to persist.</param>
    void SaveSettings(ChatHistoryOptions options);
}
