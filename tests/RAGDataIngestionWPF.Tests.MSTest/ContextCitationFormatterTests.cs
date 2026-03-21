using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ContextCitationFormatterTests
{
    [TestMethod]
    public void FormatSectionIncludesSourceLocatorAndTimestampMetadata()
    {
        IContextCitationFormatter formatter = new ContextCitationFormatter();

        string text = formatter.FormatSection(
            "Relevant local knowledge",
            [
                new ContextCitation
                {
                    Title = "SqlChatHistoryProvider",
                    SourceKind = "local-rag",
                    Locator = "42",
                    TimestampUtc = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero),
                    Content = "Provider persists and reloads chat history."
                }
            ],
            5000);

        StringAssert.Contains(text, "Relevant local knowledge:");
        StringAssert.Contains(text, "source=local-rag");
        StringAssert.Contains(text, "locator=42");
        StringAssert.Contains(text, "timestamp=2026-03-20 12:00:00Z");
        StringAssert.Contains(text, "Provider persists and reloads chat history.");
    }

    [TestMethod]
    public void FormatSectionRespectsCharacterBudget()
    {
        IContextCitationFormatter formatter = new ContextCitationFormatter();
        string large = new('x', 400);

        string text = formatter.FormatSection(
            "Relevant cached context",
            [
                new ContextCitation { Title = "first", SourceKind = "conversation-cache", Content = large },
                new ContextCitation { Title = "second", SourceKind = "conversation-cache", Content = large }
            ],
            500);

        StringAssert.Contains(text, "first");
        Assert.IsFalse(text.Contains("second", StringComparison.Ordinal));
    }
}