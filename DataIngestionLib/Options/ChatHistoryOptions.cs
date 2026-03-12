// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryOptions.cs
// Author: Kyle L. Crowder
// Build Num: 013503



namespace DataIngestionLib.Options;





/// <summary>
///     This class of settings define the behavior of the chat history and context management for a conversational AI
///     system.
///     It includes options for configuring the chat model, embeddings model, and various parameters related to how chat
///     history is retained and pruned.
///     The settings allow for customization of the chat experience, enabling features such as summarization of pruned
///     messages,
///     retention policies based on time or message count, and the inclusion of relevant external knowledge through RAG
///     (Retrieval-Augmented Generation).
///     These options are crucial for optimizing the performance and relevance of the conversational AI system in different
///     scenarios, particularly
///     These setting are to be moved to UI Registry backed .
///   This class is for the transfer of settings not a stateful container See UI Settings
/// </summary>
public sealed class ChatHistoryOptions
{
    public const string ConfigurationSectionName = "ChatHistory";

    public string ChatModelName { get; set; } = "gpt-oss:20b-cloud";

    public string ConnectionString { get; set; } = "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

    public string EmbeddingsModelName { get; set; } = "mxbai-embed-large-v1:latest";





    /// <summary>
    ///     Gets or sets the maximum number of context messages  that get injected in context.
    /// </summary>
    /// <remarks>
    ///     This property determines the limit on the number of context messages retained in memory.
    ///     Exceeding this limit may result in older messages being discarded.
    /// </remarks>
    public int MaxContextMessages { get; set; } = 40;

    /// <summary>
    ///     Sets the size of the context window before context injections.
    /// </summary>
    public int? MaxContextTokens { get; set; } = 120000;






    /// <summary>
    ///     can be used for documents, manuals, websites etc... that are relevant to the conversation but not part of the
    ///     conversation history. For example, in a customer support scenario,
    ///     RAG knowledge could include product manuals, troubleshooting guides, or FAQs that are relevant to the customer's
    ///     issue but not part of the actual conversation history. When enabled, relevant external knowledge is retrieved and
    ///     injected into the context to enhance the model's understanding and response generation.
    /// </summary>
    public bool RAGKnowledgeEnabled { get; set; } = true;


    /// <summary>
    ///     Enabled the use of past chat history message as part of the context. Can be extremely useful for enterprise
    ///     scenarios where the entire history of the conversation needs to be retained and used as part of the context for
    ///     future interactions. When enabled, past messages from the chat history are
    ///     Ideal Scenario : Call centers or support systems using LLM's to assist agents in providing better customer service
    ///     by retaining the entire history of customer interactions, allowing for more informed responses and personalized
    ///     assistance.
    ///     This is not the same as RAG Knowledge, which may consiste of in-house documents or very specific domain contents
    ///     like repair manuals, product catalogs, etc...
    /// </summary>
    public bool ChatHistoryContextEnabled { get; set; } = true;
}








public enum ContextStrategy
{
    /// <summary>
    ///     Represents a strategy for managing the context of a conversation by maintaining a sliding window
    ///     of tokens. This approach ensures that the most recent tokens are retained while older tokens
    ///     are pruned, allowing the system to stay within token limits while preserving recent context.
    /// </summary>
    SlidingTokenWindow,

    /// <summary>
    ///     Represents a fixed-size window for managing tokens within a sequence or collection.
    ///     K
    /// </summary>
    FixedTokenWindow,

    /// <summary>
    ///     Represents a context strategy where the number of messages is used
    ///     to determine the context size or limit.
    /// </summary>
    /// <remarks>enables the pruning service, must configure</remarks>
    MessageCount

}