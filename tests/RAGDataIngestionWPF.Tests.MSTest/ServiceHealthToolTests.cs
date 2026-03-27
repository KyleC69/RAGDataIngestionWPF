// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ServiceHealthToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073104



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ServiceHealthToolTests
{
    [TestMethod]
    public void ReadServicesWithInvalidMaxResultsReturnsFailure()
    {
        ServiceHealthTool tool = new();

        var result = tool.ReadServices(null, 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}