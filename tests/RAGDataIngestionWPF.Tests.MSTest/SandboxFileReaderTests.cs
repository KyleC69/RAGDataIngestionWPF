// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileReaderTests.cs
// Author: Kyle L. Crowder
// Build Num: 140925



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SandboxFileReaderTests
{
    private string _sandboxRoot = string.Empty;








    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_sandboxRoot))
        {
            Directory.Delete(_sandboxRoot, true);
        }
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ConstructorWithEmptySandboxRootThrowsArgumentException(string sandboxRoot)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new FileSystemReaderTool(sandboxRoot!));
    }








    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "reader-tool-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
    }








    [TestMethod]
    public void ReadFileAbsoluteSiblingPathReturnsAccessDenied()
    {
        var siblingFile = Path.Combine($"{_sandboxRoot}-escape", "outside.txt");
        _ = Directory.CreateDirectory(Path.GetDirectoryName(siblingFile)!);
        File.WriteAllText(siblingFile, "outside");
        FileSystemReaderTool tool = new(_sandboxRoot);

        var result = tool.ReadFile(siblingFile);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
    }








    [TestMethod]
    public void ReadFileExistingFileReturnsContent()
    {
        var filePath = Path.Combine(_sandboxRoot, "sample.txt");
        File.WriteAllText(filePath, "hello reader");
        FileSystemReaderTool tool = new FileSystemReaderTool(_sandboxRoot);

        var result = tool.ReadFile("sample.txt");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello reader", result.Value);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void ReadFileMissingFileReturnsFailure()
    {
        FileSystemReaderTool tool = new FileSystemReaderTool(_sandboxRoot);

        var result = tool.ReadFile("missing.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found: missing.txt", result.Error);
    }








    [TestMethod]
    public void ReadFileOutsideSandboxReturnsAccessDenied()
    {
        FileSystemReaderTool tool = new FileSystemReaderTool(_sandboxRoot);

        var result = tool.ReadFile("..\\outside.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void ReadFileWithNullOrWhitespacePathReturnsFailure(string relativePath)
    {
        FileSystemReaderTool tool = new FileSystemReaderTool(_sandboxRoot);

        var result = tool.ReadFile(relativePath!);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Path cannot be empty.", result.Error);
    }
}