using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class PerformanceCounterToolTests
{
    [TestMethod]
    public void ReadSnapshotWithInvalidDelayReturnsFailure()
    {
        PerformanceCounterTool tool = new();

        ToolResult<PerformanceCounterSnapshot> result = tool.ReadSnapshot(200);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("sampleDelayMilliseconds must be between 250 and 2000.", result.Error);
    }
}