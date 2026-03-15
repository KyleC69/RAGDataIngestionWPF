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



using System.Net.Http;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;





public sealed class AgentRunMiddleWare
    {

    private readonly IChatClient _baseAgent;
    private readonly ILoggerFactory _factory;








    public AgentRunMiddleWare(IChatClient baseAgent, ILoggerFactory factory, ILogger<AgentRunMiddleWare> logger, IHttpClientFactory httpClientFactory)
        {
        _baseAgent = baseAgent;
        _factory = factory;
        _ = logger;
        _ = httpClientFactory;
        }








    internal void Run()
        {
        _ = new ChatClientBuilder(_baseAgent)
                .UseLogging(_factory)
                .UseFunctionInvocation().Build();
        }
    }