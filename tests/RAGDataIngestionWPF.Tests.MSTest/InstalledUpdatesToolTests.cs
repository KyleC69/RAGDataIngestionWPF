using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class InstalledUpdatesToolTests
{
    [TestMethod]
    public void ReadInstalledUpdatesWithInvalidMaxResultsReturnsFailure()
    {
        InstalledUpdatesTool tool = new();

        ToolResult<IReadOnlyList<InstalledUpdateSnapshot>> result = tool.ReadInstalledUpdates(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}