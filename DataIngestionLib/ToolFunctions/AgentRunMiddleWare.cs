// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentRunMiddleWare.cs
// Author: Kyle L. Crowder
// Build Num: 202410



using System.Net.Http;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;





public sealed class AgentRunMiddleWare
    {

    private readonly IChatClient _baseAgent;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<AgentRunMiddleWare> _logger;








    public AgentRunMiddleWare(IChatClient baseAgent, ILoggerFactory factory, ILogger<AgentRunMiddleWare> logger, IHttpClientFactory httpClientFactory)
        {
        _baseAgent = baseAgent;
        _factory = factory;
        _logger = logger;
        }








    internal void Run()
        {

        new ChatClientBuilder(_baseAgent)
                .UseLogging(_factory)
                .UseFunctionInvocation().Build();








        }
    }