// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatSessionState.cs
// Author: Kyle L. Crowder
// Build Num: 175054



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public AIChatHistory ContextWindow { get; init; } = [];
    public AIChatHistory History { get; init; } = [];
}