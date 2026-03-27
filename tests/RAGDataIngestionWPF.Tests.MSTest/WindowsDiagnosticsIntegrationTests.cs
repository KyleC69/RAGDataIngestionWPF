// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         WindowsDiagnosticsIntegrationTests.cs
// Author: Kyle L. Crowder
// Build Num: 073109



using System.Reflection;

using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.Logging.Abstractions;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
[TestCategory("Integration")]
public class WindowsDiagnosticsIntegrationTests
{

    private static void AssertResult<T>(ToolResult<T> result, Action<T> assertSuccess)
    {
        Assert.IsNotNull(result);

        if (result.Success)
        {
            Assert.IsNotNull(result.Value);
            Assert.IsNull(result.Error);
            assertSuccess(result.Value!);
            return;
        }

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error));
        Assert.IsNull(result.Value);
    }








    [TestMethod]
    public void InstalledUpdatesToolReadInstalledUpdatesReturnsBoundedResult()
    {
        InstalledUpdatesTool tool = new();

        var result = tool.ReadInstalledUpdates(3);

        AssertResult(result, updates =>
        {
            Assert.IsNotNull(updates);
            Assert.IsTrue(updates.Count <= 3);
        });
    }








    [TestMethod]
    public void NetworkConfigurationToolReadActiveAdaptersReturnsBoundedResult()
    {
        NetworkConfigurationTool tool = new();

        var result = tool.ReadActiveAdapters(3);

        AssertResult(result, adapters =>
        {
            Assert.IsNotNull(adapters);
            Assert.IsTrue(adapters.Count <= 3);
        });
    }








    [TestMethod]
    public void PerformanceCounterToolReadSnapshotReturnsStructuredResult()
    {
        PerformanceCounterTool tool = new();

        var result = tool.ReadSnapshot(250);

        AssertResult(result, snapshot =>
        {
            Assert.IsNotNull(snapshot);
            Assert.AreEqual("250ms", snapshot.SampleWindow);
            Assert.IsTrue(snapshot.AvailableMemoryMb >= 0);
            Assert.IsTrue(snapshot.CpuPercent >= 0);
            Assert.IsTrue(snapshot.DiskQueueLength >= 0);
            Assert.IsTrue(snapshot.NetworkBytesReceivedPerSecond >= 0);
            Assert.IsTrue(snapshot.NetworkBytesSentPerSecond >= 0);
        });
    }








    [TestMethod]
    public void ProcessSnapshotToolReadTopProcessesReturnsBoundedResult()
    {
        ProcessSnapshotTool tool = new();

        var result = tool.ReadTopProcesses(5);

        AssertResult(result, processes =>
        {
            Assert.IsNotNull(processes);
            Assert.IsTrue(processes.Count <= 5);
            foreach (ProcessSnapshot process in processes)
            {
                Assert.IsTrue(process.Id > 0);
                Assert.IsFalse(string.IsNullOrWhiteSpace(process.Name));
            }
        });
    }








    [TestMethod]
    public void RegistryReaderToolReadValueReturnsStructuredResult()
    {
        RegistryReaderTool tool = new(NullLoggerFactory.Instance);

        var result = tool.ReadValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProductName");

        AssertResult(result, snapshot =>
        {
            Assert.IsNotNull(snapshot);
            Assert.AreEqual("HKEY_LOCAL_MACHINE", snapshot.Hive);
            Assert.AreEqual("ProductName", snapshot.ValueName);
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.ValueText));
        });
    }








    [TestMethod]
    public void ReliabilityHistoryToolReadRecentReturnsBoundedResult()
    {
        ReliabilityHistoryTool tool = new();

        var result = tool.ReadRecent(3);

        AssertResult(result, records =>
        {
            Assert.IsNotNull(records);
            Assert.IsTrue(records.Count <= 3);
        });
    }








    [TestMethod]
    public void ServiceHealthToolReadServicesReturnsBoundedResult()
    {
        ServiceHealthTool tool = new();

        var result = tool.ReadServices(null, 3);

        AssertResult(result, services =>
        {
            Assert.IsNotNull(services);
            Assert.IsTrue(services.Count <= 3);
            foreach (ServiceHealthSnapshot service in services) Assert.IsFalse(string.IsNullOrWhiteSpace(service.Name));
        });
    }








    [TestMethod]
    public void StartupInventoryToolReadStartupItemsReturnsBoundedResult()
    {
        StartupInventoryTool tool = new();

        var result = tool.ReadStartupItems(5);

        AssertResult(result, items =>
        {
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Count <= 5);
        });
    }








    [TestMethod]
    public void StorageHealthToolReadLogicalDisksReturnsBoundedResult()
    {
        StorageHealthTool tool = new();

        var result = tool.ReadLogicalDisks(3);

        AssertResult(result, disks =>
        {
            Assert.IsNotNull(disks);
            Assert.IsTrue(disks.Count <= 3);
            foreach (StorageHealthSnapshot disk in disks) Assert.IsFalse(string.IsNullOrWhiteSpace(disk.DeviceId));
        });
    }








    [TestMethod]
    public void ToolBuilderGetAiToolsReturnsExpandedDiagnosticsSurface()
    {
        Type toolBuilderType = Type.GetType("DataIngestionLib.ToolFunctions.ToolBuilder, DataIngestionLib")!;
        MethodInfo getAiTools = toolBuilderType.GetMethod("GetAiTools", BindingFlags.Public | BindingFlags.Static)!;

        var result = getAiTools.Invoke(null, null)!;

        Assert.IsInstanceOfType<IList<Microsoft.Extensions.AI.AITool>>(result);
        Assert.AreEqual(16, ((IList<Microsoft.Extensions.AI.AITool>)result!).Count);
    }








    [TestMethod]
    public void WindowsEventChannelReaderToolReadChannelReturnsBoundedResult()
    {
        WindowsEventChannelReaderTool tool = new();

        var result = tool.ReadChannel("System", 3);

        AssertResult(result, entries =>
        {
            Assert.IsNotNull(entries);
            Assert.IsTrue(entries.Count <= 3);
            foreach (WindowsEventChannelEntryDto entry in entries) Assert.AreEqual("System", entry.LogName);
        });
    }








    [TestMethod]
    public void WindowsWmiReaderToolReadClassReturnsBoundedResult()
    {
        WindowsWmiReaderTool tool = new();

        var result = tool.ReadClass("Win32_OperatingSystem", null, 1);

        AssertResult(result, rows =>
        {
            Assert.IsNotNull(rows);
            Assert.IsTrue(rows.Count <= 1);
            foreach (WindowsWmiInstanceDto row in rows)
            {
                Assert.AreEqual("Win32_OperatingSystem", row.ClassName);
                Assert.IsTrue(row.Properties.Count > 0);
            }
        });
    }
}