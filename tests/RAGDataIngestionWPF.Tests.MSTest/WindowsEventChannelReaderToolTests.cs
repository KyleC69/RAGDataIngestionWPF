using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class WindowsEventChannelReaderToolTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadChannelWithEmptyNameReturnsFailure(string channelName)
    {
        WindowsEventChannelReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> result = tool.ReadChannel(channelName!, 5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Channel name cannot be empty.", result.Error);
    }

    [TestMethod]
    public void ReadChannelWithUnsupportedNameReturnsFailure()
    {
        WindowsEventChannelReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> result = tool.ReadChannel("Security", 5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Channel is not allowlisted for diagnostics.", result.Error);
    }

    [TestMethod]
    public void ReadChannelWithInvalidMaxEventsReturnsFailure()
    {
        WindowsEventChannelReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> result = tool.ReadChannel("System", 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxEvents must be between 1 and 50.", result.Error);
    }
}