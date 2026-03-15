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



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
    {
    ValueTask<AIChatHistory> GetContextMessagesAsync(AIChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
    }