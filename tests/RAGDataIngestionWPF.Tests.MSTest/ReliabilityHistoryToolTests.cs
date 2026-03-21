using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ReliabilityHistoryToolTests
{
    [TestMethod]
    public void ReadRecentWithInvalidMaxResultsReturnsFailure()
    {
        ReliabilityHistoryTool tool = new();

        ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>> result = tool.ReadRecent(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}