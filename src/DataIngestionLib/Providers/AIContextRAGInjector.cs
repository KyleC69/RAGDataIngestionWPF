// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{


    private readonly ILogger<AIContextRAGInjector> _logger;

    public AIContextRAGInjector(ILogger<AIContextRAGInjector> logger)
    {
        _logger = logger;
    }







    /// <summary>
    /// Provides a collection of chat messages based on the given invoking context and available RAG (Retrieval-Augmented Generation) context sources.
    /// </summary>
    /// <param name="context">
    /// The <see cref="AIContextProvider.InvokingContext"/> containing the details of the current invocation, including request messages and session information.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/> of <see cref="ChatMessage"/> objects
    /// that are aggregated and assembled from the provided RAG context sources.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="context"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        // Validate input parameters
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        // Initialize the RAG context source
        LocalRagContextSource ragContextSource = new(_logger);
        // Retrieve the latest request message
        ChatMessage? latestRequestMessage = context.RequestMessages.LastOrDefault();
        if (latestRequestMessage == null)
        {
            return context.RequestMessages; // Return original messages if no latest message is found
        }
        // Search for RAG results based on the latest request message text
        var ragResults = ragContextSource.SearchSqlRagSource(latestRequestMessage.Text);
        // Aggregate RAG results into ChatMessage objects
        List<ChatMessage> aggregatedContext = ragResults
        .Select(result =>
        {
            ChatMessage message = new(ChatRole.System, result);
            _ = message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.External); // Tag as external source
            return message;
        })
        .ToList();
        // Combine the original request messages with the aggregated context
        return context.RequestMessages.Concat(aggregatedContext);
    }









    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        // We could remove the messages previously added to the context but 
        // we are using large sliding window for the agent's context.
        // So they will eventually fall out of the context window as the conversation continues,
        // and we want to keep them in the conversation history for traceability and debugging purposes.
        return ValueTask.CompletedTask;
    }
}