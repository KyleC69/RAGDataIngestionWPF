using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IConversationTokenCounter
{
    ConversationTokenSnapshot Calculate(IReadOnlyList<ChatMessage> history, TokenBudget budget, UsageDetails? usageDetails);
}





public readonly record struct ConversationTokenSnapshot(int Total, int Session, int Rag, int Tool, int System);