// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//

#nullable enable

using DataIngestionLib.Contracts;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;

namespace DataIngestionLib.DocIngestion;

public sealed class ChunkMetadataGenerator : IChunkMetadataGenerator
{
    private const int KeywordMaxOutputTokens = 128;
    private const int SummaryMaxOutputTokens = 384;
    private const int MaximumKeywordsLength = 500;
    private const int MaximumSummaryLength = 4000;

    private static readonly ChatOptions KeywordOptions = new()
    {
        Temperature = 0,
        MaxOutputTokens = KeywordMaxOutputTokens
    };

    private static readonly ChatOptions SummaryOptions = new()
    {
        Temperature = 0,
        MaxOutputTokens = SummaryMaxOutputTokens
    };

    private readonly IChatClient _chatClient;

    public ChunkMetadataGenerator(IAppSettings appSettings, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        if (string.IsNullOrWhiteSpace(appSettings.ChatModel))
        {
            throw new InvalidOperationException("Chat model is not configured for metadata generation.");
        }

        Uri ollamaUri = new UriBuilder(appSettings.OllamaHost) { Port = appSettings.OllamaPort }.Uri;
        IChatClient innerClient = new OllamaApiClient(ollamaUri, appSettings.ChatModel);
        _chatClient = new LoggingChatClient(innerClient, loggerFactory.CreateLogger<LoggingChatClient>());
    }

    internal ChunkMetadataGenerator(IChatClient chatClient)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        _chatClient = chatClient;
    }

    public async Task<GeneratedChunkMetadata> GenerateAsync(string chunkContent, bool includeKeywords, bool includeSummary, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chunkContent);

        if (!includeKeywords && !includeSummary)
        {
            return default;
        }

        string? keywords = null;
        string? summary = null;

        if (includeKeywords)
        {
            keywords = NormalizeKeywords(await GenerateKeywordsAsync(chunkContent, cancellationToken).ConfigureAwait(false));
        }

        if (includeSummary)
        {
            summary = NormalizeSummary(await GenerateSummaryAsync(chunkContent, cancellationToken).ConfigureAwait(false));
        }

        return new GeneratedChunkMetadata(keywords, summary);
    }

    private async Task<string> GenerateKeywordsAsync(string chunkContent, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
                                    You are a keyword extraction engine. Extract the 5-10 most important keywords from the provided text.
                                    
                                    Requirements:
                                    - Output format: comma-separated list, all lowercase.
                                    - No phrases longer than 3 words.
                                    - No filler words (the, and, of, etc.).
                                    - Prioritize nouns and technical terms.
                                    - Do NOT invent terms not present in the text.
                                    - Do NOT include duplicates.
                                    
                                    Return only the comma-separated keywords. No preamble, no labels.
                                    """;

        return await GenerateCompletionAsync(systemPrompt, chunkContent, KeywordOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GenerateSummaryAsync(string chunkContent, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
                                    You are a summarization engine. Your task is to read the provided text and produce a concise, factual summary.
                                    
                                    Requirements:
                                    - Length: 2-4 sentences.
                                    - Style: neutral, objective, and free of opinions.
                                    - Content: capture the main ideas, purpose, and important details.
                                    - Do NOT add information that is not present in the text.
                                    - Do NOT include examples unless they appear in the text.
                                    - Do NOT reference the instructions or the task itself.
                                    
                                    Return only the summary text. No preamble, no labels.
                                    """;

        return await GenerateCompletionAsync(systemPrompt, chunkContent, SummaryOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GenerateCompletionAsync(string systemPrompt, string chunkContent, ChatOptions options, CancellationToken cancellationToken)
    {
        ChatMessage[] messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, chunkContent)
        ];

        var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        var text = response.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Metadata generation returned an empty response.");
        }

        return text;
    }

    internal static string NormalizeKeywords(string keywords)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keywords);

        var normalized = string.Join(", ", keywords
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(keyword => keyword.Trim().ToLowerInvariant())
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.Ordinal));

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Metadata generation returned no usable keywords.");
        }

        if (normalized.Length <= MaximumKeywordsLength)
        {
            return normalized;
        }

        return normalized[..MaximumKeywordsLength].TrimEnd(',', ' ');
    }

    internal static string NormalizeSummary(string summary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        var normalized = summary.Trim();

        if (normalized.Length <= MaximumSummaryLength)
        {
            return normalized;
        }

        return normalized[..MaximumSummaryLength].TrimEnd();
    }
}
