// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         Tests.cs
// Author: Kyle L. Crowder
// Build Num: 140926



using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class Tests
{
    [TestMethod]
    public void ChatMessageDisplayItemCreateSetsRoleAndUserFlag()
    {
        ChatMessageDisplayItem userMessage = ChatMessageDisplayItem.Create(ChatRole.User, "hello");
        ChatMessageDisplayItem assistantMessage = ChatMessageDisplayItem.Create(ChatRole.Assistant, "hi");

        Assert.IsTrue(userMessage.IsUser);
        Assert.AreEqual(ChatRole.User.ToString(), userMessage.Role);
        Assert.AreEqual("hello", userMessage.Text);

        Assert.IsFalse(assistantMessage.IsUser);
        Assert.AreEqual(ChatRole.Assistant.ToString(), assistantMessage.Role);
        Assert.AreEqual("hi", assistantMessage.Text);
    }








    [TestMethod]
    public async Task SampleDataServiceReturnsOrderCollectionsForBothConsumers()
    {
        SampleDataService service = new();

        var gridOrderIds = (await service.GetGridDataAsync()).Select(order => order.OrderId).ToList();
        var listDetailsOrderIds = (await service.GetListDetailsDataAsync()).Select(order => order.OrderId).ToList();

        Assert.IsTrue(gridOrderIds.Count > 0);
        CollectionAssert.AreEqual(gridOrderIds, listDetailsOrderIds);
    }
}