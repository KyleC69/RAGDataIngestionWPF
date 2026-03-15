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




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistorySummarizer
    {
    ValueTask<AIChatMessage?> SummarizeAsync(string conversationId, AIChatHistory messages, CancellationToken cancellationToken = default);
    }