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



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IVectorChatHistoryProvider
    {
    IReadOnlyList<string> StateKeys { get; }


    object? GetService(Type serviceType, object? serviceKey);


    T? GetService<T>(T? serviceKey);


    string GetSessionDbKey(AgentSession session);


    ValueTask InvokedAsync(ChatHistoryProvider.InvokedContext context, CancellationToken cancellationToken);


    ValueTask<IEnumerable<ChatMessage>> InvokingAsync(ChatHistoryProvider.InvokingContext context, CancellationToken cancellationToken);
    }