// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ConfigurationAndEdgeCaseServiceTests.cs
// Author: Kyle L. Crowder
// Build Num: 073048



using System.Reflection;

using DataIngestionLib.Contracts;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Settings;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ConfigurationAndEdgeCaseServiceTests
{

    [TestMethod]
    public void AppSettingsGetTokenBudgetAggregatesConfiguredBudgets()
    {
        AppSettings settings = new();

        var oldSession = settings.SessionBudget;
        var oldSystem = settings.SystemBudget;
        var oldRag = settings.RAGBudget;
        var oldTool = settings.ToolBudget;
        var oldMeta = settings.MetaBudget;
        var oldContext = settings.MaximumContext;

        try
        {
            settings.SessionBudget = 10;
            settings.SystemBudget = 20;
            settings.RAGBudget = 30;
            settings.ToolBudget = 40;
            settings.MetaBudget = 50;
            settings.MaximumContext = 900;

            TokenBudget tokenBudget = settings.GetTokenBudget();


            Assert.AreEqual(10, tokenBudget.SessionBudget);
            Assert.AreEqual(20, tokenBudget.SystemBudget);
            Assert.AreEqual(30, tokenBudget.RAGBudget);
            Assert.AreEqual(40, tokenBudget.ToolBudget);
            Assert.AreEqual(50, tokenBudget.MetaBudget);
            Assert.AreEqual(900, tokenBudget.MaximumContext);
            Assert.AreEqual(150, tokenBudget.BudgetTotal);
        }
        finally
        {
            settings.SessionBudget = oldSession;
            settings.SystemBudget = oldSystem;
            settings.RAGBudget = oldRag;
            settings.ToolBudget = oldTool;
            settings.MetaBudget = oldMeta;
            settings.MaximumContext = oldContext;
        }
    }








    [TestMethod]
    public void AppSettingsStringSettersNormalizeNullToEmpty()
    {
        AppSettings settings = new AppSettings();
        var oldChatModel = settings.ChatModel;
        var oldAgentId = settings.AgentId;

        try
        {
            settings.ChatModel = null;
            settings.AgentId = null;
            Assert.AreEqual(string.Empty, settings.ChatModel);
            Assert.AreEqual(string.Empty, settings.AgentId);
        }
        finally
        {
            settings.ChatModel = oldChatModel;
            settings.AgentId = oldAgentId;
        }
    }








    [TestMethod]
    public void IdentityCacheServiceRoundTripsEncryptedToken()
    {
        Type serviceType = Type.GetType("RAGDataIngestionWPF.Services.IdentityCacheService, RAGDataIngestionWPF");
        Assert.IsNotNull(serviceType);

        var service = Activator.CreateInstance(serviceType, true);

        FieldInfo pathField = serviceType.GetField("MsalCacheFilePath", BindingFlags.Public | BindingFlags.Static);
        FieldInfo fileField = serviceType.GetField("MsalCacheFileName", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(pathField);
        Assert.IsNotNull(fileField);

        var folder = (string)pathField.GetValue(null);
        var file = (string)fileField.GetValue(null);
        var fullPath = Path.Combine(folder, file);

        var previous = File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;

        try
        {
            byte[] token = [1, 2, 3, 4, 5, 6];
            _ = serviceType.GetMethod("SaveMsalToken", BindingFlags.Public | BindingFlags.Instance).Invoke(service, new object[] { token });
            var roundTrip = (byte[])serviceType.GetMethod("ReadMsalToken", BindingFlags.Public | BindingFlags.Instance).Invoke(service, null);

            CollectionAssert.AreEqual(token, roundTrip);
        }
        finally
        {
            if (previous == null)
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            else
            {
                _ = Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, previous);
            }
        }
    }








    [TestMethod]
    public void SettingsViewModelPrivateParseMethodsHandleEdgeCases()
    {
        Type vmType = typeof(SettingsViewModel);

        MethodInfo parseBool = vmType.GetMethod("ParseBool", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseInt = vmType.GetMethod("ParseInt", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseNullableInt = vmType.GetMethod("ParseNullableInt", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseTheme = vmType.GetMethod("ParseTheme", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsTrue((bool)parseBool.Invoke(null, new object[] { "true", false }));
        Assert.IsFalse((bool)parseBool.Invoke(null, new object[] { "invalid", false }));

        Assert.AreEqual(12, (int)parseInt.Invoke(null, new object[] { "12", 3, 1 }));
        Assert.AreEqual(3, (int)parseInt.Invoke(null, new object[] { "0", 3, 1 }));

        Assert.IsNull(parseNullableInt.Invoke(null, new object[] { "", 5 }));
        Assert.AreEqual(5, (int)parseNullableInt.Invoke(null, new object[] { "-4", 5 }));

        var theme = parseTheme.Invoke(null, new object[] { "NotATheme" });
        Assert.AreEqual(Models.AppTheme.Default, (Models.AppTheme)theme);
    }








    [TestMethod]
    public void SettingsViewModelSetThemeCommandCanSurfaceMissingThemeResourceEdgeCase()
    {
        StaTestHelper.Run(() =>
        {
            Mock<ISystemService> system = new Mock<ISystemService>();
            Mock<IApplicationInfoService> appInfo = new Mock<IApplicationInfoService>();
            appInfo.Setup(service => service.GetVersion()).Returns(new Version(1, 2, 3, 4));
            Mock<IUserDataService> userData = new Mock<IUserDataService>();

            SettingsViewModel viewModel = new SettingsViewModel(system.Object, appInfo.Object, userData.Object);

            Exception captured = null;
            try
            {
                viewModel.SetThemeCommand.Execute("Dark");
            }
            catch (Exception ex)
            {
                captured = ex;
            }

            Assert.IsNotNull(captured);
        });
    }








    [TestMethod]
    public void SystemServiceOpenInWebBrowserWithNullUrlThrows()
    {
        SystemService service = new SystemService();

        Exception captured = null;
        try
        {
            service.OpenInWebBrowser(null);
        }
        catch (Exception ex)
        {
            captured = ex;
        }

        Assert.IsNotNull(captured);
    }
}