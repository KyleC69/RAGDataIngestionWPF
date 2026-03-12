// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatSessionState.cs
// Author: Kyle L. Crowder
// Build Num: 013458



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public AIChatHistory ContextWindow { get; init; } = [];
    public AIChatHistory History { get; init; } = [];
}