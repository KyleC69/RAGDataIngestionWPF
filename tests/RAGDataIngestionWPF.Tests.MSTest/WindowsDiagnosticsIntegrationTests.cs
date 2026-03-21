using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.Logging.Abstractions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
[TestCategory("Integration")]
public class WindowsDiagnosticsIntegrationTests
{
    [TestMethod]
    public void PerformanceCounterToolReadSnapshotReturnsStructuredResult()
    {
        PerformanceCounterTool tool = new();

        ToolResult<PerformanceCounterSnapshot> result = tool.ReadSnapshot(250);

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
    public void ServiceHealthToolReadServicesReturnsBoundedResult()
    {
        ServiceHealthTool tool = new();

        ToolResult<IReadOnlyList<ServiceHealthSnapshot>> result = tool.ReadServices(null, 3);

        AssertResult(result, services =>
        {
            Assert.IsNotNull(services);
            Assert.IsTrue(services.Count <= 3);
            foreach (ServiceHealthSnapshot service in services)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(service.Name));
            }
        });
    }

    [TestMethod]
    public void ReliabilityHistoryToolReadRecentReturnsBoundedResult()
    {
        ReliabilityHistoryTool tool = new();

        ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>> result = tool.ReadRecent(3);

        AssertResult(result, records =>
        {
            Assert.IsNotNull(records);
            Assert.IsTrue(records.Count <= 3);
        });
    }

    [TestMethod]
    public void InstalledUpdatesToolReadInstalledUpdatesReturnsBoundedResult()
    {
        InstalledUpdatesTool tool = new();

        ToolResult<IReadOnlyList<InstalledUpdateSnapshot>> result = tool.ReadInstalledUpdates(3);

        AssertResult(result, updates =>
        {
            Assert.IsNotNull(updates);
            Assert.IsTrue(updates.Count <= 3);
        });
    }

    [TestMethod]
    public void StartupInventoryToolReadStartupItemsReturnsBoundedResult()
    {
        StartupInventoryTool tool = new();

        ToolResult<IReadOnlyList<StartupInventoryEntry>> result = tool.ReadStartupItems(5);

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

        ToolResult<IReadOnlyList<StorageHealthSnapshot>> result = tool.ReadLogicalDisks(3);

        AssertResult(result, disks =>
        {
            Assert.IsNotNull(disks);
            Assert.IsTrue(disks.Count <= 3);
            foreach (StorageHealthSnapshot disk in disks)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(disk.DeviceId));
            }
        });
    }

    [TestMethod]
    public void NetworkConfigurationToolReadActiveAdaptersReturnsBoundedResult()
    {
        NetworkConfigurationTool tool = new();

        ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>> result = tool.ReadActiveAdapters(3);

        AssertResult(result, adapters =>
        {
            Assert.IsNotNull(adapters);
            Assert.IsTrue(adapters.Count <= 3);
        });
    }

    [TestMethod]
    public void ProcessSnapshotToolReadTopProcessesReturnsBoundedResult()
    {
        ProcessSnapshotTool tool = new();

        ToolResult<IReadOnlyList<ProcessSnapshot>> result = tool.ReadTopProcesses(5);

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
    public void WindowsEventChannelReaderToolReadChannelReturnsBoundedResult()
    {
        WindowsEventChannelReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> result = tool.ReadChannel("System", 3);

        AssertResult(result, entries =>
        {
            Assert.IsNotNull(entries);
            Assert.IsTrue(entries.Count <= 3);
            foreach (WindowsEventChannelEntryDto entry in entries)
            {
                Assert.AreEqual("System", entry.LogName);
            }
        });
    }

    [TestMethod]
    public void WindowsWmiReaderToolReadClassReturnsBoundedResult()
    {
        WindowsWmiReaderTool tool = new();

        ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> result = tool.ReadClass("Win32_OperatingSystem", null, 1);

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

    [TestMethod]
    public void RegistryReaderToolReadValueReturnsStructuredResult()
    {
        RegistryReaderTool tool = new(NullLoggerFactory.Instance);

        ToolResult<RegistryValueSnapshot> result = tool.ReadValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProductName");

        AssertResult(result, snapshot =>
        {
            Assert.IsNotNull(snapshot);
            Assert.AreEqual("HKEY_LOCAL_MACHINE", snapshot.Hive);
            Assert.AreEqual("ProductName", snapshot.ValueName);
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.ValueText));
        });
    }

    [TestMethod]
    public void ToolBuilderGetAiToolsReturnsExpandedDiagnosticsSurface()
    {
        Type toolBuilderType = Type.GetType("DataIngestionLib.ToolFunctions.ToolBuilder, DataIngestionLib")!;
        var getAiTools = toolBuilderType.GetMethod("GetAiTools", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;

        object result = getAiTools.Invoke(null, null)!;

        Assert.IsInstanceOfType<IList<Microsoft.Extensions.AI.AITool>>(result);
        Assert.AreEqual(16, ((IList<Microsoft.Extensions.AI.AITool>)result!).Count);
    }

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
}