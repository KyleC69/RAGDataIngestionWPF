using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class StorageHealthToolTests
{
    [TestMethod]
    public void ReadLogicalDisksWithInvalidMaxResultsReturnsFailure()
    {
        StorageHealthTool tool = new();

        ToolResult<IReadOnlyList<StorageHealthSnapshot>> result = tool.ReadLogicalDisks(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }
}