// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SandboxFileWriterTests.cs
// Author: Kyle L. Crowder
// Build Num: 140925



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SandboxFileWriterTests
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
        Assert.ThrowsExactly<ArgumentException>(() => _ = new FileSystemWriterTool(sandboxRoot!));
    }








    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "writer-tool-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
    }








    [TestMethod]
    public void WriteTextAbsoluteSiblingPathReturnsAccessDenied()
    {
        var siblingDirectory = $"{_sandboxRoot}-escape";
        _ = Directory.CreateDirectory(siblingDirectory);
        FileSystemWriterTool tool = new(_sandboxRoot);

        var result = tool.WriteText(Path.Combine(siblingDirectory, "outside.txt"), "unsafe");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
        Assert.IsFalse(File.Exists(Path.Combine(siblingDirectory, "outside.txt")));
    }








    [TestMethod]
    public void WriteTextMissingIntermediateDirectoryReturnsIoError()
    {
        FileSystemWriterTool tool = new FileSystemWriterTool(_sandboxRoot);

        var result = tool.WriteText("folder\\file.txt", "value");

        Assert.IsFalse(result.Success);
        StringAssert.StartsWith(result.Error, "I/O error writing file 'folder\\file.txt':");
    }








    [TestMethod]
    public void WriteTextOutsideSandboxReturnsAccessDenied()
    {
        FileSystemWriterTool tool = new FileSystemWriterTool(_sandboxRoot);

        var result = tool.WriteText("..\\outside.txt", "unsafe");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied: path is outside the sandbox.", result.Error);
    }








    [TestMethod]
    public void WriteTextValidPathWritesContentAndReturnsSuccess()
    {
        var targetPath = Path.Combine(_sandboxRoot, "out.txt");
        FileSystemWriterTool tool = new FileSystemWriterTool(_sandboxRoot);

        var result = tool.WriteText("out.txt", "hello writer");

        Assert.IsTrue(result.Success);
        Assert.AreEqual($"Wrote {targetPath}", result.Value);
        Assert.IsTrue(File.Exists(targetPath));
        Assert.AreEqual("hello writer", File.ReadAllText(targetPath));
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void WriteTextWithNullOrWhitespacePathReturnsFailure(string relativePath)
    {
        FileSystemWriterTool tool = new FileSystemWriterTool(_sandboxRoot);

        var result = tool.WriteText(relativePath!, "content");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Path cannot be null or whitespace.", result.Error);
    }
}