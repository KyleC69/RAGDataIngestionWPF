using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class NetworkConfigurationToolTests
{
    [TestMethod]
    public void ReadActiveAdaptersWithInvalidMaxResultsReturnsFailure()
    {
        NetworkConfigurationTool tool = new();

        ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>> result = tool.ReadActiveAdapters(0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 12.", result.Error);
    }
}