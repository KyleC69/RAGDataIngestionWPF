using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ServiceHealthToolTests
{
    [TestMethod]
    public void ReadServicesWithInvalidMaxResultsReturnsFailure()
    {
        ServiceHealthTool tool = new();

        ToolResult<IReadOnlyList<ServiceHealthSnapshot>> result = tool.ReadServices(null, 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 25.", result.Error);
    }
}