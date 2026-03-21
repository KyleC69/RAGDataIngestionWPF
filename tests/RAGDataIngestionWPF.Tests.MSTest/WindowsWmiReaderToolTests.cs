using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class WindowsWmiReaderToolTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadClassWithEmptyClassNameReturnsFailure(string className)
    {
        WindowsWmiReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> result = tool.ReadClass(className!, null, 5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class name cannot be empty.", result.Error);
    }

    [TestMethod]
    public void ReadClassWithUnsupportedClassReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> result = tool.ReadClass("Win32_Process", null, 5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class is not allowlisted for diagnostics.", result.Error);
    }

    [TestMethod]
    public void ReadClassWithInvalidMaxResultsReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> result = tool.ReadClass("Win32_OperatingSystem", null, 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }

    [TestMethod]
    public void ReadClassWithUnsupportedPropertyReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> result = tool.ReadClass("Win32_OperatingSystem", "InstallDate", 5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Property 'InstallDate' is not allowlisted for class 'Win32_OperatingSystem'.", result.Error);
    }
}