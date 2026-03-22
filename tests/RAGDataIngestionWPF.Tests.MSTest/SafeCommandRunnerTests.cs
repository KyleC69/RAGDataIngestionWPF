// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SafeCommandRunnerTests.cs
// Author: Kyle L. Crowder
// Build Num: 140927



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SafeCommandRunnerTests
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








    [TestInitialize]
    public void Initialize()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "safe-command-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_sandboxRoot);
        File.WriteAllText(Path.Combine(_sandboxRoot, "sample.txt"), "sample content");
    }








    [TestMethod]
    public void RunCatAbsoluteSiblingPathReturnsAccessDenied()
    {
        var siblingFile = Path.Combine($"{_sandboxRoot}-escape", "outside.txt");
        _ = Directory.CreateDirectory(Path.GetDirectoryName(siblingFile)!);
        File.WriteAllText(siblingFile, "outside");
        SafeCommandRunner runner = new(_sandboxRoot);

        var result = runner.Run($"cat {siblingFile}");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied.", result.Error);
    }








    [TestMethod]
    public void RunCatExistingFileReturnsContents()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("cat sample.txt");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("sample content", result.Value);
    }








    [TestMethod]
    public void RunCatMissingFileReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("cat missing.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("File not found.", result.Error);
    }








    [TestMethod]
    public void RunCatOutsideSandboxReturnsAccessDenied()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("cat ..\\outside.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Access denied.", result.Error);
    }








    [TestMethod]
    public void RunDirReturnsSandboxFileNames()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("dir");

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.Value, "sample.txt");
    }








    [TestMethod]
    public void RunEchoReturnsProvidedText()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("echo hello world");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello world", result.Value);
    }








    [TestMethod]
    public void RunWithDisallowedCommandReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("del sample.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Command 'del' is not allowed.", result.Error);
    }








    [TestMethod]
    public void RunWithEmptyInputReturnsFailure()
    {
        SafeCommandRunner runner = new SafeCommandRunner(_sandboxRoot);

        var result = runner.Run("   ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("No command provided.", result.Error);
    }
}