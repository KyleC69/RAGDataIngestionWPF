// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         WindowsWmiReaderToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073111



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

        var result = tool.ReadClass(className!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class name cannot be empty.", result.Error);
    }








    [TestMethod]
    public void ReadClassWithInvalidMaxResultsReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        var result = tool.ReadClass("Win32_OperatingSystem", null, 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be between 1 and 20.", result.Error);
    }








    [TestMethod]
    public void ReadClassWithUnsupportedClassReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        var result = tool.ReadClass("Win32_Process");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class is not allowlisted for diagnostics.", result.Error);
    }








    [TestMethod]
    public void ReadClassWithUnsupportedPropertyReturnsFailure()
    {
        WindowsWmiReaderTool tool = new();

        var result = tool.ReadClass("Win32_OperatingSystem", "InstallDate");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Property 'InstallDate' is not allowlisted for class 'Win32_OperatingSystem'.", result.Error);
    }
}