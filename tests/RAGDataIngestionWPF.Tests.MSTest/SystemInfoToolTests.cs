// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SystemInfoToolTests.cs
// Author: Kyle L. Crowder
// Build Num: 073105



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SystemInfoToolTests
{
    [TestMethod]
    public void GetInfoReturnsSnapshotWithExpectedFields()
    {
        var result = SystemInfoTool.GetInfo();

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.Os));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.MachineName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.DotNetVersion));
        Assert.IsTrue(result.Value.ProcessorCount > 0);
        Assert.IsNull(result.Error);
    }
}