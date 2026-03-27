// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         RagQueryExpanderTests.cs
// Author: Kyle L. Crowder
// Build Num: 073102



using DataIngestionLib.Contracts;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class RagQueryExpanderTests
{
    [TestMethod]
    public void ExpandReturnsPrimaryAndKeywordQuery()
    {
        IRagQueryExpander expander = new RagQueryExpander();

        var queries = expander.Expand([
                new(ChatRole.User, "How does SQL chat history schema validation work at startup?")
        ]);

        Assert.AreEqual(2, queries.Count);
        Assert.AreEqual("How does SQL chat history schema validation work at startup?", queries[0].Query);
        Assert.AreEqual("SQL chat history schema validation work startup", queries[1].Query);
    }








    [TestMethod]
    public void ExpandUsesLatestUserMessageOnly()
    {
        IRagQueryExpander expander = new RagQueryExpander();

        var queries = expander.Expand([
                new(ChatRole.User, "older question"),
                new(ChatRole.Assistant, "assistant reply"),
                new(ChatRole.User, "latest query terms")
        ]);

        Assert.AreEqual("latest query terms", queries[0].Query);
    }
}