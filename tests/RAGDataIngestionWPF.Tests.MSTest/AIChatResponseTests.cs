using System;
using System.Diagnostics;
using System.Windows.Forms;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Agents.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

using Windows.System;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.Providers;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Settings;




namespace RAGDataIngestionWPF.Tests.MSTest;


[TestClass]
public class AgentFuzzerTests
{
    internal AgentSession _session;
    internal ProviderSessionState<HistoryIdentity> _provider;







    public AgentFuzzerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AIChatHistoryDb>();
        services.AddSingleton<IAgentFactory, AgentFactory>();
        services.AddScoped<IChatHistoryProvider, SqlChatHistoryProvider>();
        services.AddScoped<ISQLChatHistoryProvider, SqlChatHistoryProvider>();
        services.AddSingleton<IAppSettings, AppSettings>();
        services.AddSingleton<ChatHistoryContextInjector>();
        IServiceCollection unused3 = services.AddSingleton<SqlChatHistoryProvider>();
        IServiceCollection unused4 = services.AddSingleton<IChatHistoryProvider>(provider => provider.GetRequiredService<SqlChatHistoryProvider>());
        IServiceCollection unused5 = services.AddSingleton<ISQLChatHistoryProvider>(provider => provider.GetRequiredService<SqlChatHistoryProvider>());
        _ = services.AddSingleton<IConversationHistoryContextOrchestrator, ConversationHistoryContextOrchestrator>();
        services.AddDbContext<AIChatHistoryDb>();
        services.AddDbContext<RAGContext>();

        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var appSettings = serviceProvider.GetRequiredService<IAppSettings>();
        var sqlChatHistoryProvider = serviceProvider.GetRequiredService<ISQLChatHistoryProvider>();
        var chatHistoryContextInjector = new ChatHistoryContextInjector(loggerFactory.CreateLogger<ChatHistoryContextInjector>());
        //  var conversationContextCacheRecorder = new ConversationContextCacheRecorder(new NullConversationContextCacheStore(), loggerFactory.CreateLogger<ConversationContextCacheRecorder>());

        _agentFactory = serviceProvider.GetRequiredService<IAgentFactory>();

        _agent = _agentFactory.GetCodingAssistantAgent("Agent101", "gpt-oss:20b-cloud", "", null);
    }








    internal static AIAgent _agent;


    private readonly Random _rng = new();
    private readonly IAgentFactory _agentFactory;

    private static readonly string[] Actions = new[]
    {
            "summarize", "rewrite", "explain", "convert", "extract",
            "summarize",
            "rewrite",
            "explain",
            "convert",
            "extract",
            "translate",
            "improve",
            "simplify"
    };

    private static readonly string[] Inputs = new[]
    {
            "this text about fishing reels",
            "this SQL query: SELECT * FROM Orders",
            "this JSON payload: { \"id\": 1 }",
            "this C# code snippet: public class A {}",
            "this error message: IndexOutOfRangeException",
            "this text about fishing reels",
            "this SQL query: SELECT * FROM Users WHERE Age > 30",
            "this JSON payload: { \"name\": \"Kyle\", \"role\": \"founder\" }",
            "this C# code snippet: public record Person(string Name);",
            "this error message: NullReferenceException at line 42",
            "this paragraph about AI agents and orchestration",
            "this list of items: apples, oranges, pears",
            "this markdown document with a header and bullet points"
    };

    private string GenerateRandomTask()
    {
        var action = Actions[_rng.Next(Actions.Length)];
        var input = Inputs[_rng.Next(Inputs.Length)];
        return $"{action} {input}";
    }


    public async Task Agent_Should_Respond_Stably()
    {
        var pstate = new ProviderSessionState<HistoryIdentity>(
                 stateInitializer: (session) => new HistoryIdentity
                 {
                     AgentId = _agent.Id,
                     ApplicationId = "RAGDataIngestionWPF",
                     ConversationId = Guid.NewGuid().ToString(),
                     UserId = "TestUser"
                 },
                 stateKey: "HistoryIdentity");

        var identity = pstate.GetOrInitializeState(_session);
        pstate.SaveState(_session, identity);

        var historyProvider = new SqlChatHistoryProvider(new NullLogger<SqlChatHistoryProvider>(), new AppSettings(), new AIChatHistoryDb());

        const int iterations = 5;

        for (int i = 0; i < iterations; i++)
        {
            var instruction = GenerateRandomTask();

            var sw = Stopwatch.StartNew();
            var result = await _agent.RunAsync(instruction, _session);
            sw.Stop();

            await Task.Delay(8000);
            // --- 2. Fetch the last message from ChatHistoryProvider ---
            var history = await historyProvider.GetLastMessageAsync();
            Assert.IsNotNull(history, "ChatHistoryProvider returned null history");
            Assert.IsTrue(!string.IsNullOrEmpty(history.Content), "ChatHistoryProvider returned no messages");


            // 1. Basic null check
            Assert.IsNotNull(result, "Agent returned null result");

            // 2. Output must not be empty
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Text),
                    "Agent returned empty output");

            // 3. No safety hallucinations
            Assert.IsFalse(result.Text.Contains("illegal or harmful"),
                    "Agent hallucinated a safety refusal");

            Assert.IsFalse(result.Text.Contains("I can't help with that"),
                    "Agent drifted into safety refusal mode");

            // 4. No meta‑chat drift
            Assert.IsFalse(result.Text.Contains("As an AI"),
                    "Agent drifted into meta‑chat");

            // 5. Latency sanity check
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000,
                    $"Agent took too long: {sw.ElapsedMilliseconds}ms");
        }
    }




    [ClassInitialize]
    public static void Initializer(TestContext context)
    {
        var loggerFactory = new LoggerFactory();
        var appSettings = new AppSettings();
        var sqlChatHistoryProvider = new SqlChatHistoryProvider(new NullLogger<SqlChatHistoryProvider>(), appSettings, new AIChatHistoryDb());
        var chatHistoryContextInjector = new ChatHistoryContextInjector(loggerFactory.CreateLogger<ChatHistoryContextInjector>());



    }





    [TestMethod]
    public async Task RunRandomizedAgentTasks_FillsChatHistory()
    {
        // TODO: Replace with however you construct your agent

        const int taskCount = 1; // adjust as needed

        for (int i = 0; i < taskCount; i++)
        {
            var instruction = GenerateRandomTask();

            var result = await _agent.RunAsync(instruction);

            // Optional: assert nothing exploded
            Assert.IsNotNull(result);

            // Optional: small delay to simulate real usage
            await Task.Delay(10);
        }
    }
}









