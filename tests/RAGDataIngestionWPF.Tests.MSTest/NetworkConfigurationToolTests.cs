// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         NetworkConfigurationToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073101



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class NetworkConfigurationToolTests
{
    [TestMethod]
    public void ReadActiveAdaptersWithInvalidMaxResultsReturnsFailure()
    {
        NetworkConfigurationTool tool = new();

        var result = tool.ReadActiveAdapters(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 12.", result.Error);
    }
}