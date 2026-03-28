// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         RagContextMessageAssemblerTests.cs
// Author: Kyle L. Crowder
// Build Num: 073102

using DataIngestionLib.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class RagContextMessageAssemblerTests
{
    [TestMethod]
    public void TokenizeSkipsShortTermsAndDeduplicatesIgnoringCase()
    {
        string[] terms = ConversationHistoryContextOrchestrator.Tokenize("AI ai runtime Runtime the docs docs retrieval");

        CollectionAssert.AreEquivalent(new[] { "runtime", "the", "docs", "retrieval" }, terms);
        Assert.IsFalse(terms.Contains("AI", StringComparer.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ScoreIncreasesWhenQueryAndTermsArePresent()
    {
        const string content = "This runtime retrieval guide explains query expansion and retrieval details.";
        const string query = "retrieval guide";

        double score = ConversationHistoryContextOrchestrator.Score(content, query, new[] { "runtime", "retrieval" });

        Assert.IsTrue(score >= 12d);
    }
}
