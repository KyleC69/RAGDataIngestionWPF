// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextMessageAssembler.cs
// Author: Kyle L. Crowder
// Build Num: 140745



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextMessageAssembler
{
    IReadOnlyList<ChatMessage> Assemble(IReadOnlyList<ChatMessage> requestMessages, IReadOnlyList<ChatMessage> candidateMessages);
}