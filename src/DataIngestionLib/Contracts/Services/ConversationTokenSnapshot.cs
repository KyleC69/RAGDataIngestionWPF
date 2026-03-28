// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ConversationTokenSnapshot.cs
// Author: Kyle L. Crowder
// Build Num: 072939

namespace DataIngestionLib.Contracts.Services;

public readonly record struct ConversationTokenSnapshot(int Total, int Session, int Rag, int Tool, int System);
