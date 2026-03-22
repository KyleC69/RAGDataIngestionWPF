// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         PerformanceCounterToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 141005



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class PerformanceCounterToolTests
{
    [TestMethod]
    public void ReadSnapshotWithInvalidDelayReturnsFailure()
    {
        PerformanceCounterTool tool = new();

        var result = tool.ReadSnapshot(200);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("sampleDelayMilliseconds must be between 250 and 2000.", result.Error);
    }
}