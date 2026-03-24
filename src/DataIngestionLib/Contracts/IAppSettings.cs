// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



namespace DataIngestionLib.Contracts;





public interface IAppSettings
{
    string? AgentId { get; set; }

    string? ApplicationId { get; set; }

    string ChatHistoryConnectionString { get; set; }

    string ChatModel { get; set; }

    string EmbeddingModel { get; set; }
    string? LastConversationId { get; set; }

    string? LearnBaseUrl { get; set; }

    string? LogDirectory { get; set; }
    string? LogName { get; set; }
    int MaximumContext { get; set; }
    int MetaBudget { get; set; }

    string OllamaHost { get; set; }

    int OllamaPort { get; set; }
    int RAGBudget { get; set; }

    string RemoteRAGConnectionString { get; set; }
    bool ResumeLast { get; set; }
    int SessionBudget { get; set; }
    int SystemBudget { get; set; }
    int ToolBudget { get; set; }
    string UserId { get; set; }


    TokenBudget GetTokenBudget();


    void SetValue(string conversationid, string conversationId);
}