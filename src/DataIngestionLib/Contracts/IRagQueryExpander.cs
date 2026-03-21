using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts;

public interface IRagQueryExpander
{
    IReadOnlyList<RagSearchQuery> Expand(IReadOnlyList<ChatMessage> requestMessages);
}