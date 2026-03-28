// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ContextCitationFormatterTests.cs
// Author: Kyle L. Crowder
// Build Num: 073049

using DataIngestionLib.Models;
using DataIngestionLib.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ContextCitationFormatterTests
{
    [TestMethod]
    public void FormatSectionIncludesSourceLocatorAndTimestampMetadata()
    {
        ContextCitationFormatter formatter = new();

        var text = formatter.FormatSection("Relevant local knowledge", [
                new ContextCitation
                {
                        Title = "SqlChatHistoryProvider",
                        SourceKind = "local-rag",
                        Locator = "42",
                        TimestampUtc = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero),
                        Content = "Provider persists and reloads chat history."
                }
        ], 5000);

        StringAssert.Contains(text, "Relevant local knowledge:");
        StringAssert.Contains(text, "source=local-rag");
        StringAssert.Contains(text, "locator=42");
        StringAssert.Contains(text, "timestamp=2026-03-20 12:00:00Z");
        StringAssert.Contains(text, "Provider persists and reloads chat history.");
    }

    [TestMethod]
    public void FormatSectionRespectsCharacterBudget()
    {
        ContextCitationFormatter formatter = new();
        string large = new('x', 400);

        var text = formatter.FormatSection("Relevant cached context", [
                new ContextCitation { Title = "first", SourceKind = "conversation-cache", Content = large },
                new ContextCitation { Title = "second", SourceKind = "conversation-cache", Content = large }
        ], 500);

        StringAssert.Contains(text, "first");
        Assert.IsFalse(text.Contains("second", StringComparison.Ordinal));
    }
}
