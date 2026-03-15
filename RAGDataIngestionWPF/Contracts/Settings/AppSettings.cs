// Build Date: 2026/03/14
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         AppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 013053

using DataIngestionLib.Contracts;





namespace RAGDataIngestionWPF.Contracts.Settings;





public class AppSettings : IAppSettings
{
    /// <inheritdoc />
    public string OllamaHost =>Properties.Settings.Default.OllamaHost;
    

    /// <inheritdoc />
    public int OllamaPort => Properties.Settings.Default.OllamaPort;

    /// <inheritdoc />
    public string ChatModel => Properties.Settings.Default.ChatModel;

    /// <inheritdoc />
    public string EmbeddingModel => Properties.Settings.Default.EmbeddingModel;

    /// <inheritdoc />
    public string LearnBaseUrl => Properties.Settings.Default.LearnBaseUrl;

    /// <inheritdoc />
    public string LogDirectory => Properties.Settings.Default.LogDirectory;

    /// <inheritdoc />
    public string ChatHistoryConnectionString => Environment.GetEnvironmentVariable("CHAT_HISTORY") ?? string.Empty;
    
    public string RemoteRAGConnectionString => Environment.GetEnvironmentVariable("REMOTE_RAG") ?? string.Empty;


}