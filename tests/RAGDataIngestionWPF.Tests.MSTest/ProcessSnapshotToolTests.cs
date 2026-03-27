// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ProcessSnapshotToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073101



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ProcessSnapshotToolTests
{
    [TestMethod]
    public void ReadTopProcessesWithInvalidMaxResultsReturnsFailure()
    {
        ProcessSnapshotTool tool = new();

        var result = tool.ReadTopProcesses(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }
}