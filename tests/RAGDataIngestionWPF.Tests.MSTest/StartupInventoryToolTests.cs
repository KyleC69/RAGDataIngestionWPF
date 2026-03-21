using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class StartupInventoryToolTests
{
    [TestMethod]
    public void ReadStartupItemsWithInvalidMaxResultsReturnsFailure()
    {
        StartupInventoryTool tool = new();

        ToolResult<IReadOnlyList<StartupInventoryEntry>> result = tool.ReadStartupItems(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 40.", result.Error);
    }
}