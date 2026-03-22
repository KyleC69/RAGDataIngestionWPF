// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         StartupInventoryToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 141005



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class StartupInventoryToolTests
{
    [TestMethod]
    public void ReadStartupItemsWithInvalidMaxResultsReturnsFailure()
    {
        StartupInventoryTool tool = new();

        var result = tool.ReadStartupItems(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 40.", result.Error);
    }
}