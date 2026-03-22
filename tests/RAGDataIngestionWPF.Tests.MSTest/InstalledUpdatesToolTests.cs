// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         InstalledUpdatesToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 141004



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class InstalledUpdatesToolTests
{
    [TestMethod]
    public void ReadInstalledUpdatesWithInvalidMaxResultsReturnsFailure()
    {
        InstalledUpdatesTool tool = new();

        var result = tool.ReadInstalledUpdates(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}