// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         LocalRagContextSourceTests.cs
// Author: Kyle L. Crowder
// Build Num: 073058

using DataIngestionLib.Providers;

using Microsoft.Extensions.Logging.Abstractions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class LocalRagContextSourceTests
{
    [TestMethod]
    public void SearchSqlRagSourceReturnsEmptyWhenConnectionStringMissing()
    {
        string? original = Environment.GetEnvironmentVariable("REMOTE_RAG");

        try
        {
            Environment.SetEnvironmentVariable("REMOTE_RAG", null);
            LocalRagContextSource source = new(NullLogger<AIContextRAGInjector>.Instance);

            string[] result = source.SearchSqlRagSource("question");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }
        finally
        {
            Environment.SetEnvironmentVariable("REMOTE_RAG", original);
        }
    }

    [TestMethod]
    public void SearchSqlRagSourceReturnsEmptyWhenConnectionStringInvalid()
    {
        string? original = Environment.GetEnvironmentVariable("REMOTE_RAG");

        try
        {
            Environment.SetEnvironmentVariable("REMOTE_RAG", "Server=invalid;Database=missing;Integrated Security=true;");
            LocalRagContextSource source = new(NullLogger<AIContextRAGInjector>.Instance);

            string[] result = source.SearchSqlRagSource("latest question");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }
        finally
        {
            Environment.SetEnvironmentVariable("REMOTE_RAG", original);
        }
    }
}
