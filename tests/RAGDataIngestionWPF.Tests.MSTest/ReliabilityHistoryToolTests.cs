// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ReliabilityHistoryToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073103



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ReliabilityHistoryToolTests
{
    [TestMethod]
    public void ReadRecentWithInvalidMaxResultsReturnsFailure()
    {
        ReliabilityHistoryTool tool = new();

        var result = tool.ReadRecent(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}