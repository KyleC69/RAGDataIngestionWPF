// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIContextRAGInjector.cs
// Author: Kyle L. Crowder
// Build Num: 044246



using DataIngestionLib.Contracts.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{
    private readonly IRagContextMessageAssembler _assembler;
    private readonly IConversationContextCacheStore _cacheStore;
    private readonly IReadOnlyList<IRagContextSource> _sources;








    public AIContextRAGInjector(
            IEnumerable<IRagContextSource> sources,
            IRagContextMessageAssembler assembler,
            IConversationContextCacheStore cacheStore)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(assembler);
        ArgumentNullException.ThrowIfNull(cacheStore);
        _sources = sources.ToArray();
        _assembler = assembler;
        _cacheStore = cacheStore;
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        List<ChatMessage> requestMessages =
        [
                .. context.RequestMessages
                        .Select(m => new ChatMessage(m.Role, m.Text))
        ];
        if (_sources.Count == 0)
        {
            return [];
        }

        List<ChatMessage> aggregatedContext = [];
        foreach (IRagContextSource source in _sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceMessages = await source
                    .GetContextMessagesAsync(requestMessages, context.Session, cancellationToken)
                    .ConfigureAwait(false);

            if (sourceMessages.Count == 0)
            {
                continue;
            }

            aggregatedContext.AddRange(sourceMessages.Where(static message => !string.IsNullOrWhiteSpace(message.Text)));
        }

        IReadOnlyList<ChatMessage> assembled = _assembler.Assemble(requestMessages, aggregatedContext);
        string conversationId = ResolveConversationId(context.Session);
        if (!string.IsNullOrWhiteSpace(conversationId) && assembled.Count > 0)
        {
            await _cacheStore.AppendAsync(conversationId, assembled, cancellationToken).ConfigureAwait(false);
        }

        return assembled;
    }








    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    private static string ResolveConversationId(AgentSession? session)
    {
        if (session is null)
        {
            return string.Empty;
        }

        if (session.StateBag.TryGetValue("ConversationId", out string? conversationId)
            && !string.IsNullOrWhiteSpace(conversationId))
        {
            return conversationId;
        }

        if (session.StateBag.TryGetValue("ChatHistoryConversationId", out string? chatHistoryConversationId)
            && !string.IsNullOrWhiteSpace(chatHistoryConversationId))
        {
            return chatHistoryConversationId;
        }

        return string.Empty;
    }
}