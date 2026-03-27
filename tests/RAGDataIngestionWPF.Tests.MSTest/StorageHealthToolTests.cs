// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         StorageHealthToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073105



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class StorageHealthToolTests
{
    [TestMethod]
    public void ReadLogicalDisksWithInvalidMaxResultsReturnsFailure()
    {
        StorageHealthTool tool = new();

        var result = tool.ReadLogicalDisks(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }
}