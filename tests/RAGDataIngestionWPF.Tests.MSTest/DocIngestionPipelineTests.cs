// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         DocIngestionPipelineTests.cs
// Author: GitHub Copilot
// Build Num: 000000

using System.Text.RegularExpressions;

using DataIngestionLib.DocIngestion;

using Microsoft.Extensions.Logging.Abstractions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class DocIngestionPipelineTests
{
    [TestMethod]
    public void BuildChunksSkipsFencedCodeBlocksAndProducesTextOnlyChunks()
    {
        const string markdown = "# Title\nIntro paragraph.\n```csharp\nvar x = 1;\n```\nAfter paragraph.";

        IReadOnlyList<DocIngestionPipeline.ChunkPayload> chunks = DocIngestionPipeline.BuildChunks(markdown);

        Assert.AreEqual(2, chunks.Count);
        Assert.IsTrue(chunks.All(chunk => chunk.ChunkType == "text"));
        Assert.IsTrue(chunks.All(chunk => chunk.Language is null));
        Assert.AreEqual("Intro paragraph.", chunks[0].Content);
        Assert.AreEqual("After paragraph.", chunks[1].Content);
    }

    [TestMethod]
    public async Task ParseMarkdownDocumentAsyncFlattensLinksAndNormalizesText()
    {
        ChunkMetadataGenerator generator = new(NullLoggerFactory.Instance);
        DocIngestionPipeline pipeline = new(NullLoggerFactory.Instance, generator);

        var tempFile = Path.Combine(Path.GetTempPath(), $"DocIngestionPipelineTests_{Guid.NewGuid():N}.md");

        try
        {
            var markdown = """
                ---
                title: Sample
                ---

                # Heading

                Visit [Docs](https://example.com), <https://contoso.example>, and <xref:sample-xref>.
                Escaped: \*bold\* and \[list\].

                :::code source="snippet.cs" language="csharp"
                :::

                ```json
                { ""k"": 1 }
                ```

                Mixed     whitespace  here.
                """;

            await File.WriteAllTextAsync(tempFile, markdown).ConfigureAwait(false);

            DocIngestionPipeline.ParsedDocumentPayload payload = await pipeline.ParseMarkdownDocumentAsync(tempFile, CancellationToken.None).ConfigureAwait(false);

            Assert.IsTrue(payload.NormalizedMarkdown.Contains("Docs (https://example.com)", StringComparison.Ordinal));
            Assert.IsTrue(payload.NormalizedMarkdown.Contains("https://contoso.example", StringComparison.Ordinal));
            Assert.IsTrue(payload.NormalizedMarkdown.Contains("sample-xref", StringComparison.Ordinal));
            Assert.IsTrue(payload.NormalizedMarkdown.Contains("Escaped: *bold* and [list].", StringComparison.Ordinal));
            Assert.IsTrue(payload.NormalizedMarkdown.Contains("Mixed whitespace here.", StringComparison.Ordinal));

            Assert.IsFalse(payload.NormalizedMarkdown.Contains("```", StringComparison.Ordinal));
            Assert.IsFalse(payload.NormalizedMarkdown.Contains(":::code", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(payload.NormalizedMarkdown.Contains("\u0007", StringComparison.Ordinal));
            Assert.IsFalse(Regex.IsMatch(payload.NormalizedMarkdown, " {2,}"));
            Assert.IsTrue(payload.Chunks.All(chunk => chunk.ChunkType == "text"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
