// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;


using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;



namespace DataIngestionLib.DocIngestion;





public sealed class DocIngestionPipeline
{
    private const string StartFolder = "e:\\IngestionSource\\docs\\docs\\";
    private const int MaxIncludeDepth = 8;

    private static readonly Regex HeadingRegex = new(@"^(#{1,6})\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex FencedCodeStartRegex = new(@"^```(?<lang>[^\s`]*)", RegexOptions.Compiled);
    private static readonly Regex DirectiveAttributeRegex = new("(?<key>[A-Za-z0-9_-]+)\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.Compiled);
    private static readonly Regex MarkdownImageRegex = new(@"!\[(?<text>[^\]]*)\]\((?<url>[^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkRegex = new(@"\[(?<text>[^\]]+)\]\((?<url>[^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex HtmlLinkRegex = new(@"<(?<url>https?://[^>]+)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex XrefRegex = new(@"<xref:(?<xref>[^>]+)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public DocIngestionPipeline(IAgentFactory agentfactory, ILoggerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(agentfactory);
        ArgumentNullException.ThrowIfNull(factory);

        _agentFactory = agentfactory;
        _factory = factory;
        _logger = _factory.CreateLogger<DocIngestionPipeline>();
    }

    private readonly IAgentFactory _agentFactory;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<DocIngestionPipeline> _logger;


    public Task DoIngestionsAsync()
    {
        return DoIngestionAsync();
    }

    public async Task DoIngestionAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(StartFolder))
        {
            _logger.LogWarning("Markdown ingestion folder does not exist: {StartFolder}", StartFolder);
            return;
        }

        string[] markdownFiles = Directory
            .GetFiles(StartFolder, "*.md", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _logger.LogInformation("Found {FileCount} markdown files for ingestion in {StartFolder}", markdownFiles.Length, StartFolder);

        foreach (string filePath in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ParsedDocumentPayload parsedDocument = await ParseMarkdownDocumentAsync(filePath, cancellationToken).ConfigureAwait(false);

                await SaveDocumentSqlStubAsync(parsedDocument, cancellationToken).ConfigureAwait(false);
                await SaveChunksSqlStubAsync(parsedDocument.DocumentId, parsedDocument.Chunks, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest markdown document: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Markdown ingestion completed.");
    }

    private async Task<ParsedDocumentPayload> ParseMarkdownDocumentAsync(string filePath, CancellationToken cancellationToken)
    {
        string rawFileContent = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        (string yamlFrontMatter, string markdownBody) = ExtractYamlFrontMatter(rawFileContent);

        HashSet<string> includeStack = new(StringComparer.OrdinalIgnoreCase);
        string resolvedMarkdown = await ResolveDocFxDirectivesAsync(filePath, markdownBody, includeStack, 0, cancellationToken).ConfigureAwait(false);
        string normalizedMarkdownBody = FlattenHyperlinks(resolvedMarkdown);
        string normalizedDocument = string.IsNullOrWhiteSpace(yamlFrontMatter)
            ? normalizedMarkdownBody
            : $"{yamlFrontMatter}{Environment.NewLine}{Environment.NewLine}{normalizedMarkdownBody}";

        IReadOnlyList<ChunkPayload> chunks = BuildChunks(normalizedMarkdownBody);

        string relativePath = Path.GetRelativePath(StartFolder, filePath);

        return new ParsedDocumentPayload(
            Guid.NewGuid(),
            relativePath,
            Path.GetFileName(filePath),
            yamlFrontMatter,
            rawFileContent,
            normalizedDocument,
            chunks);
    }

    private async Task<string> ResolveDocFxDirectivesAsync(
        string owningFilePath,
        string markdown,
        HashSet<string> includeStack,
        int depth,
        CancellationToken cancellationToken)
    {
        if (depth > MaxIncludeDepth)
        {
            _logger.LogWarning("Max include recursion depth reached while processing {FilePath}", owningFilePath);
            return markdown;
        }

        string normalizedPath = Path.GetFullPath(owningFilePath);
        if (!includeStack.Add(normalizedPath))
        {
            _logger.LogWarning("Circular include detected for {FilePath}; include skipped.", owningFilePath);
            return string.Empty;
        }

        try
        {
            StringBuilder output = new();
            string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            int index = 0;

            while (index < lines.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string line = lines[index];
                string trimmed = line.Trim();

                if (trimmed.StartsWith(":::code", StringComparison.OrdinalIgnoreCase))
                {
                    string directiveText = CollectDirectiveText(lines, ref index);
                    string codeBlock = await ResolveCodeDirectiveAsync(owningFilePath, directiveText, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(codeBlock))
                    {
                        output.AppendLine(codeBlock);
                    }

                    continue;
                }

                if (trimmed.StartsWith(":::include", StringComparison.OrdinalIgnoreCase))
                {
                    string directiveText = CollectDirectiveText(lines, ref index);
                    string includeContent = await ResolveIncludeDirectiveAsync(owningFilePath, directiveText, includeStack, depth + 1, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(includeContent))
                    {
                        output.AppendLine(includeContent);
                    }

                    continue;
                }

                output.AppendLine(line);
                index++;
            }

            return output.ToString();
        }
        finally
        {
            includeStack.Remove(normalizedPath);
        }
    }

    private async Task<string> ResolveIncludeDirectiveAsync(
        string owningFilePath,
        string directiveText,
        HashSet<string> includeStack,
        int depth,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> attributes = ParseDirectiveAttributes(directiveText);
        if (!attributes.TryGetValue("file", out string? includePath) &&
            !attributes.TryGetValue("path", out includePath) &&
            !attributes.TryGetValue("source", out includePath))
        {
            _logger.LogDebug(":::include directive in {FilePath} does not declare file/path/source.", owningFilePath);
            return string.Empty;
        }

        string includeAbsolutePath = ResolveRelativePath(owningFilePath, includePath);
        if (!File.Exists(includeAbsolutePath))
        {
            _logger.LogWarning("Included file not found: {IncludePath} referenced by {FilePath}", includeAbsolutePath, owningFilePath);
            return string.Empty;
        }

        string includeMarkdown = await File.ReadAllTextAsync(includeAbsolutePath, cancellationToken).ConfigureAwait(false);
        return await ResolveDocFxDirectivesAsync(includeAbsolutePath, includeMarkdown, includeStack, depth, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> ResolveCodeDirectiveAsync(string owningFilePath, string directiveText, CancellationToken cancellationToken)
    {
        Dictionary<string, string> attributes = ParseDirectiveAttributes(directiveText);
        if (!attributes.TryGetValue("source", out string? sourcePath))
        {
            _logger.LogDebug(":::code directive in {FilePath} does not include source= attribute.", owningFilePath);
            return string.Empty;
        }

        string language = attributes.TryGetValue("language", out string? languageValue)
            ? languageValue
            : string.Empty;

        string absoluteSourcePath = ResolveRelativePath(owningFilePath, sourcePath);
        if (!File.Exists(absoluteSourcePath))
        {
            _logger.LogWarning("Code source file not found: {SourcePath} referenced by {FilePath}", absoluteSourcePath, owningFilePath);
            return string.Empty;
        }

        string code = await File.ReadAllTextAsync(absoluteSourcePath, cancellationToken).ConfigureAwait(false);

        if (attributes.TryGetValue("id", out string? snippetId) && !string.IsNullOrWhiteSpace(snippetId))
        {
            string snippet = ExtractSnippetById(code, snippetId);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                code = snippet;
            }
        }

        return BuildCodeFence(language, code);
    }

    private static string ExtractSnippetById(string sourceCode, string snippetId)
    {
        string startMarker = $"<\u002F?{snippetId}>";
        MatchCollection markerMatches = Regex.Matches(sourceCode, startMarker, RegexOptions.IgnoreCase);

        if (markerMatches.Count < 2)
        {
            return sourceCode;
        }

        int start = markerMatches[0].Index + markerMatches[0].Length;
        int end = markerMatches[1].Index;
        if (end <= start)
        {
            return sourceCode;
        }

        return sourceCode[start..end].Trim();
    }

    private static string BuildCodeFence(string language, string code)
    {
        StringBuilder builder = new();
        builder.Append("```");
        if (!string.IsNullOrWhiteSpace(language))
        {
            builder.Append(language.Trim());
        }

        builder.AppendLine();
        builder.AppendLine(code.TrimEnd());
        builder.Append("```");
        return builder.ToString();
    }

    private static string CollectDirectiveText(string[] lines, ref int index)
    {
        StringBuilder builder = new();
        string firstLine = lines[index];
        builder.AppendLine(firstLine);

        if (firstLine.TrimEnd().EndsWith(":::", StringComparison.Ordinal))
        {
            index++;
            return builder.ToString();
        }

        index++;
        while (index < lines.Length)
        {
            string line = lines[index];
            builder.AppendLine(line);
            index++;

            if (line.Trim() == ":::")
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static Dictionary<string, string> ParseDirectiveAttributes(string directiveText)
    {
        Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in DirectiveAttributeRegex.Matches(directiveText))
        {
            string key = match.Groups["key"].Value;
            string value = match.Groups["value"].Value;

            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, value);
            }
        }

        return attributes;
    }

    private static string ResolveRelativePath(string owningFilePath, string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        string owningDirectory = Path.GetDirectoryName(owningFilePath) ?? StartFolder;
        return Path.GetFullPath(Path.Combine(owningDirectory, path));
    }

    private static string FlattenHyperlinks(string markdown)
    {
        string flattened = MarkdownImageRegex.Replace(markdown, m => m.Groups["text"].Value.Trim());
        flattened = MarkdownLinkRegex.Replace(flattened, m =>
        {
            string text = m.Groups["text"].Value.Trim();
            string url = m.Groups["url"].Value.Trim();
            return string.IsNullOrWhiteSpace(text) ? url : $"{text} ({url})";
        });
        flattened = HtmlLinkRegex.Replace(flattened, m => m.Groups["url"].Value.Trim());
        flattened = XrefRegex.Replace(flattened, m => m.Groups["xref"].Value.Trim());

        return flattened;
    }

    private static (string yamlFrontMatter, string body) ExtractYamlFrontMatter(string markdown)
    {
        string normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (string.Empty, markdown);
        }

        int end = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return (string.Empty, markdown);
        }

        string yaml = normalized[..(end + 5)].TrimEnd();
        string body = normalized[(end + 5)..].TrimStart('\n');

        return (yaml, body);
    }

    private static IReadOnlyList<ChunkPayload> BuildChunks(string markdownBody)
    {
        List<ChunkPayload> chunks = new();
        List<(int Level, string Heading)> headingStack = new();

        string[] lines = markdownBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        StringBuilder prose = new();
        int chunkIndex = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            Match headingMatch = HeadingRegex.Match(line);
            if (headingMatch.Success)
            {
                FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);

                int level = headingMatch.Groups[1].Value.Length;
                string heading = headingMatch.Groups[2].Value.Trim();

                while (headingStack.Count > 0 && headingStack[^1].Level >= level)
                {
                    headingStack.RemoveAt(headingStack.Count - 1);
                }

                headingStack.Add((level, heading));
                continue;
            }

            Match fenceMatch = FencedCodeStartRegex.Match(line);
            if (fenceMatch.Success)
            {
                FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);

                string language = fenceMatch.Groups["lang"].Value.Trim();
                List<string> codeLines = new();
                i++;

                while (i < lines.Length && !lines[i].StartsWith("```", StringComparison.Ordinal))
                {
                    codeLines.Add(lines[i]);
                    i++;
                }

                if (codeLines.Count > 1)
                {
                    string codeBlock = string.Join(Environment.NewLine, codeLines).Trim();
                    if (!string.IsNullOrWhiteSpace(codeBlock))
                    {
                        string headingPath = BuildHeadingPath(headingStack);
                        string heading = headingStack.Count == 0 ? string.Empty : headingStack[^1].Heading;
                        int? headingLevel = headingStack.Count == 0 ? null : headingStack[^1].Level;

                        chunks.Add(new ChunkPayload(
                            chunkIndex,
                            heading,
                            headingLevel,
                            headingPath,
                            "code",
                            language,
                            codeBlock,
                            EstimateTokenCount(codeBlock)));

                        chunkIndex++;
                    }
                }

                continue;
            }

            prose.AppendLine(line);
        }

        FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);
        return chunks;
    }

    private static void FlushProseChunk(
        ICollection<ChunkPayload> chunks,
        StringBuilder prose,
        IReadOnlyList<(int Level, string Heading)> headingStack,
        ref int chunkIndex)
    {
        string content = prose.ToString().Trim();
        prose.Clear();

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        string headingPath = BuildHeadingPath(headingStack);
        string heading = headingStack.Count == 0 ? string.Empty : headingStack[^1].Heading;
        int? headingLevel = headingStack.Count == 0 ? null : headingStack[^1].Level;

        chunks.Add(new ChunkPayload(
            chunkIndex,
            heading,
            headingLevel,
            headingPath,
            "text",
            null,
            content,
            EstimateTokenCount(content)));

        chunkIndex++;
    }

    private static string BuildHeadingPath(IReadOnlyList<(int Level, string Heading)> headingStack)
    {
        if (headingStack.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(" > ", headingStack.Select(x => x.Heading));
    }

    private static int EstimateTokenCount(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 0;
        }

        int wordCount = content
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return Math.Max(1, (int)Math.Ceiling(wordCount * 1.35));
    }

    private Task SaveDocumentSqlStubAsync(ParsedDocumentPayload document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(
            "SQL STUB save document: {RelativePath}, RawLength={RawLength}, NormalizedLength={NormalizedLength}, Chunks={ChunkCount}",
            document.RelativePath,
            document.RawMarkdown.Length,
            document.NormalizedMarkdown.Length,
            document.Chunks.Count);
        return Task.CompletedTask;
    }

    private Task SaveChunksSqlStubAsync(Guid documentId, IReadOnlyList<ChunkPayload> chunks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("SQL STUB save chunks: DocumentId={DocumentId}, ChunkCount={ChunkCount}", documentId, chunks.Count);
        return Task.CompletedTask;
    }

    private sealed record ParsedDocumentPayload(
        Guid DocumentId,
        string RelativePath,
        string FileName,
        string YamlFrontMatter,
        string RawMarkdown,
        string NormalizedMarkdown,
        IReadOnlyList<ChunkPayload> Chunks);

    private sealed record ChunkPayload(
        int ChunkIndex,
        string Heading,
        int? HeadingLevel,
        string HeadingPath,
        string ChunkType,
        string? Language,
        string Content,
        int TokenCount);
}