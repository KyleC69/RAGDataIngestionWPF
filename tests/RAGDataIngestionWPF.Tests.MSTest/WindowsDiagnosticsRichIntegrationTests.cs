// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         WindowsDiagnosticsRichIntegrationTests.cs
// Author: Kyle L. Crowder
// Build Num: 141010



using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.Logging.Abstractions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
[TestCategory("Integration")]
public class WindowsDiagnosticsRichIntegrationTests
{

    private static void AssertToolSuccess<T>(ToolResult<T> result, Action<T> assertSuccess)
    {
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success, result.Error);
        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Value);
        assertSuccess(result.Value!);
    }








    [TestMethod]
    public void NetworkConfigurationToolReturnsTrimmedActiveAdapterDetails()
    {
        NetworkConfigurationTool tool = new();

        var result = tool.ReadActiveAdapters(2);

        AssertToolSuccess(result, adapters =>
        {
            Assert.IsTrue(adapters.Count <= 2);
            foreach (NetworkConfigurationSnapshot adapter in adapters)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(adapter.AdapterName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(adapter.OperationalStatus));
                Assert.IsTrue(adapter.DnsServers.Length <= 256);
                Assert.IsTrue(adapter.GatewayAddresses.Length <= 256);
                Assert.IsTrue(adapter.UnicastAddresses.Length <= 256);
            }
        });
    }








    [TestMethod]
    public void PerformanceCounterToolSnapshotUsesRequestedWindow()
    {
        PerformanceCounterTool tool = new();

        var result = tool.ReadSnapshot(250);

        AssertToolSuccess(result, snapshot =>
        {
            Assert.AreEqual("250ms", snapshot.SampleWindow);
            Assert.IsTrue(snapshot.AvailableMemoryMb >= 0);
            Assert.IsTrue(snapshot.CpuPercent is >= 0 and <= 1000);
        });
    }








    [TestMethod]
    public void RegistryReaderToolReadWindowsProductNameReturnsNamedValue()
    {
        RegistryReaderTool tool = new(NullLoggerFactory.Instance);

        var result = tool.ReadValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProductName");

        AssertToolSuccess(result, snapshot =>
        {
            Assert.AreEqual("HKEY_LOCAL_MACHINE", snapshot.Hive);
            Assert.AreEqual("ProductName", snapshot.ValueName);
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.KeyPath));
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.ValueKind));
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.ValueText));
        });
    }








    [TestMethod]
    public void SystemInfoToolGetInfoMatchesCurrentMachine()
    {
        var result = SystemInfoTool.GetInfo();

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(Environment.MachineName, result.Value.MachineName);
        Assert.AreEqual(Environment.ProcessorCount, result.Value.ProcessorCount);
        Assert.AreEqual(Environment.Version.ToString(), result.Value.DotNetVersion);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.Os));
    }








    [TestMethod]
    public void WindowsEventChannelReaderToolSystemChannelReturnsStructuredEntries()
    {
        WindowsEventChannelReaderTool tool = new();

        var result = tool.ReadChannel("System", 2);

        AssertToolSuccess(result, entries =>
        {
            Assert.IsTrue(entries.Count <= 2);
            foreach (WindowsEventChannelEntryDto entry in entries)
            {
                Assert.AreEqual("System", entry.LogName);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Level));
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.ProviderName) && string.IsNullOrWhiteSpace(entry.Message));
            }
        });
    }








    [TestMethod]
    public void WindowsWmiReaderToolOperatingSystemReturnsExpectedProperties()
    {
        WindowsWmiReaderTool tool = new();

        var result = tool.ReadClass("Win32_OperatingSystem", "Caption,Version,LastBootUpTime", 1);

        AssertToolSuccess(result, rows =>
        {
            Assert.AreEqual(1, rows.Count);
            WindowsWmiInstanceDto row = rows[0];
            Assert.AreEqual("Win32_OperatingSystem", row.ClassName);
            Assert.IsTrue(row.Properties.ContainsKey("Caption"));
            Assert.IsTrue(row.Properties.ContainsKey("Version"));
            Assert.IsTrue(row.Properties.ContainsKey("LastBootUpTime"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(row.Properties["Caption"]));
            Assert.IsFalse(string.IsNullOrWhiteSpace(row.Properties["Version"]));
        });
    }
}