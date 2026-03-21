using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ProcessSnapshotToolTests
{
    [TestMethod]
    public void ReadTopProcessesWithInvalidMaxResultsReturnsFailure()
    {
        ProcessSnapshotTool tool = new();

        ToolResult<IReadOnlyList<ProcessSnapshot>> result = tool.ReadTopProcesses(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }
}