// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 140742



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null);
}