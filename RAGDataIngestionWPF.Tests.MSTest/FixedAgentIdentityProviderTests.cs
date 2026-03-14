// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         FixedAgentIdentityProviderTests.cs
// Author: Kyle L. Crowder
// Build Num: 202418



using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





/// <summary>
///     Unit tests for <see cref="FixedAgentIdentityProvider" /> and related model correctness.
/// </summary>
[TestClass]
public class FixedAgentIdentityProviderTests
{




    [TestMethod]
    public void AIChatMessageToStringReturnsEmptyStringWhenContentIsNull()
    {
        AIChatMessage message = new(ChatRole.User, (string)null);
        Assert.AreEqual(string.Empty, message.ToString());
    }








    [TestMethod]
    public void AIChatMessageToStringReturnsMessageText()
    {
        AIChatMessage message = new(ChatRole.User, "Hello, world!");
        Assert.AreEqual("Hello, world!", message.ToString());
    }








    [TestMethod]
    public void GetAgentIdReturnsConfiguredValue()
    {
        FixedAgentIdentityProvider provider = new("my-agent");
        Assert.AreEqual("my-agent", provider.GetAgentId());
    }
}