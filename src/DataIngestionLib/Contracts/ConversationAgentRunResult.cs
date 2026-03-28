// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationAgentRunResult.cs
// Author: Kyle L. Crowder
// Build Num: 072938

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts;

public readonly record struct ConversationAgentRunResult(string Text, UsageDetails? UsageDetails);
