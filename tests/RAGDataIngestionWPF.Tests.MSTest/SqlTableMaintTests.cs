using DataIngestionLib.Contracts;
using DataIngestionLib.DocIngestion;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class SqlTableMaintTests
{
    [TestMethod]
    public async Task ProcessPendingChunksAsyncContinuesAfterChunkFailure()
    {
        Mock<IChunkMetadataGenerator> metadataGenerator = new(MockBehavior.Strict);
        SqlTableMaint.PendingChunk[] pendingChunks =
        [
            new(Guid.NewGuid(), Guid.NewGuid(), 0, "alpha content", true, true),
            new(Guid.NewGuid(), Guid.NewGuid(), 1, "beta content", true, false)
        ];

        metadataGenerator
            .Setup(generator => generator.GenerateAsync("alpha content", true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedChunkMetadata("alpha, content", "Alpha summary."));

        metadataGenerator
            .Setup(generator => generator.GenerateAsync("beta content", true, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("generation failed"));

        SqlTableMaint maint = new(() => "Server=(localdb)\\MSSQLLocalDB;Database=ignored;Integrated Security=true;", metadataGenerator.Object, NullLogger<SqlTableMaint>.Instance);
        List<Guid> persistedChunkIds = [];

        MetadataUpdateResult result = await maint.ProcessPendingChunksAsync(
            pendingChunks,
            (chunk, metadata, cancellationToken) =>
            {
                persistedChunkIds.Add(chunk.ChunkId);
                Assert.AreEqual("alpha, content", metadata.Keywords);
                Assert.AreEqual("Alpha summary.", metadata.Summary);
                return Task.CompletedTask;
            }).ConfigureAwait(false);

        Assert.AreEqual(1, result.UpdatedCount);
        Assert.AreEqual(1, result.FailedCount);
        CollectionAssert.AreEquivalent(new[] { pendingChunks[0].ChunkId }, persistedChunkIds);
    }

    [TestMethod]
    public async Task ProcessPendingChunksAsyncHonorsCancellationBeforeBatchStarts()
    {
        Mock<IChunkMetadataGenerator> metadataGenerator = new(MockBehavior.Strict);
        SqlTableMaint.PendingChunk[] pendingChunks =
        [
            new(Guid.NewGuid(), Guid.NewGuid(), 0, "alpha content", true, true)
        ];

        SqlTableMaint maint = new(() => "Server=(localdb)\\MSSQLLocalDB;Database=ignored;Integrated Security=true;", metadataGenerator.Object, NullLogger<SqlTableMaint>.Instance);
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => maint.ProcessPendingChunksAsync(
            pendingChunks,
            static (_, _, _) => Task.CompletedTask,
            cancellationTokenSource.Token)).ConfigureAwait(false);

        metadataGenerator.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task UpdateMetadataAsyncThrowsWhenConnectionStringIsMissing()
    {
        Mock<IChunkMetadataGenerator> metadataGenerator = new(MockBehavior.Strict);
        SqlTableMaint maint = new(() => " ", metadataGenerator.Object, NullLogger<SqlTableMaint>.Instance);

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => maint.UpdateMetadataAsync(CancellationToken.None)).ConfigureAwait(false);

        Assert.AreEqual("Remote RAG connection string is not configured.", exception.Message);
        metadataGenerator.VerifyNoOtherCalls();
    }
}
