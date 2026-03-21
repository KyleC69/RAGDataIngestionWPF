using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;




public interface IRagContextMessageAssembler
{
    IReadOnlyList<ChatMessage> Assemble(IReadOnlyList<ChatMessage> requestMessages, IReadOnlyList<ChatMessage> candidateMessages);
}