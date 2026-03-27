// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         RagContextMessageAssemblerTests.cs
// Author: Kyle L. Crowder
// Build Num: 073102



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class RagContextMessageAssemblerTests
{

    [TestMethod]
    public void AssembleSkipsDuplicateCandidateTextsAndPreservesOrder()
    {
        IRagContextMessageAssembler assembler = new RagContextMessageAssembler(CreateSettings().Object);
        var assembled = assembler.Assemble([], [
                new(new ChatRole(AIChatRole.RAGContext.Value), "first"),
                new(new ChatRole(AIChatRole.RAGContext.Value), "first"),
                new(new ChatRole(AIChatRole.RAGContext.Value), "second")
        ]);

        Assert.AreEqual(2, assembled.Count);
        Assert.AreEqual("first", assembled[0].Text);
        Assert.AreEqual("second", assembled[1].Text);
    }








    [TestMethod]
    public void AssembleSkipsDuplicatesAlreadyPresentInRequestMessages()
    {
        IRagContextMessageAssembler assembler = new RagContextMessageAssembler(CreateSettings().Object);
        IReadOnlyList<ChatMessage> requestMessages =
        [
                new(ChatRole.User, "repeat me")
        ];
        IReadOnlyList<ChatMessage> candidateMessages =
        [
                new(new ChatRole(AIChatRole.RAGContext.Value), "repeat me"),
                new(new ChatRole(AIChatRole.RAGContext.Value), "new context")
        ];

        var assembled = assembler.Assemble(requestMessages, candidateMessages);

        Assert.AreEqual(1, assembled.Count);
        Assert.AreEqual("new context", assembled[0].Text);
    }








    [TestMethod]
    public void AssembleStopsWhenCharacterBudgetWouldBeExceeded()
    {
        IRagContextMessageAssembler assembler = new RagContextMessageAssembler(CreateSettings(ragBudget: 125).Object);
        string large = new('x', 600);

        var assembled = assembler.Assemble([], [
                new(new ChatRole(AIChatRole.RAGContext.Value), large),
                new(new ChatRole(AIChatRole.RAGContext.Value), "second")
        ]);

        Assert.AreEqual(1, assembled.Count);
        Assert.AreEqual(large, assembled[0].Text);
    }








    [TestMethod]
    public void AssembleTreatsWhitespaceVariantsAsDuplicates()
    {
        IRagContextMessageAssembler assembler = new RagContextMessageAssembler(CreateSettings().Object);

        var assembled = assembler.Assemble([new(ChatRole.User, "Relevant local knowledge: First block")], [new(new ChatRole(AIChatRole.RAGContext.Value), "Relevant   local knowledge:\nFirst block")]);

        Assert.AreEqual(0, assembled.Count);
    }








    private static Mock<IAppSettings> CreateSettings(int ragBudget = 1000)
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(x => x.RAGBudget).Returns(ragBudget);
        return settings;
    }
}