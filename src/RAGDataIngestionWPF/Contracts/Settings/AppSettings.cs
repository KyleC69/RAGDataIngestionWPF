// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//

using DataIngestionLib.Contracts;
using DataIngestionLib.Services.Contracts;







namespace RAGDataIngestionWPF.Contracts.Settings;








public class AppSettings : IAppSettings
    {
    /// <inheritdoc />
    public string OllamaHost => Properties.Settings.Default.OllamaHost;


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
    public int SessionBudget => Properties.Settings.Default.SessionBudget;
    public int SystemBudget => Properties.Settings.Default.SystemBudget;
    public int RAGBudget => Properties.Settings.Default.RAGBudget;
    public int ToolBudget => Properties.Settings.Default.ToolBudget;
    public int MetaBudget => Properties.Settings.Default.MetaBudget;
    public int MaximumContext => Properties.Settings.Default.MaximumContext;



    public TokenBudget GetTokenBudget()
        {
        return new TokenBudget
            {
            SessionBudget = Properties.Settings.Default.SessionBudget,
            SystemBudget = Properties.Settings.Default.SystemBudget,
            RAGBudget = Properties.Settings.Default.RAGBudget,
            ToolBudget = Properties.Settings.Default.ToolBudget,
            MetaBudget = Properties.Settings.Default.MetaBudget,
            BudgetTotal = SessionBudget + SystemBudget + RAGBudget + ToolBudget + MetaBudget,
            MaximumContext = Properties.Settings.Default.MaximumContext
            };
        }
    }