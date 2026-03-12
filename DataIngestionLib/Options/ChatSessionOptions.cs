// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatSessionOptions.cs
// Author: Kyle L. Crowder
// Build Num: 013503



namespace DataIngestionLib.Options;





public sealed class ChatSessionOptions
{

    public string ChatSessionFileName { get; set; } = "ChatSession.json";
    public string ConfigurationsFolder { get; set; } = string.Empty;

    public int MaxContextTokens { get; set; } = 120000;
}