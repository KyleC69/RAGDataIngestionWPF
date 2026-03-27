// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagQueryExpander.cs
// Author: Kyle L. Crowder
// Build Num: 072940



using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts;





public interface IRagQueryExpander
{
    IReadOnlyList<RagSearchQuery> Expand(IReadOnlyList<ChatMessage> requestMessages);
}