// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//

#nullable enable

using System.Data;

using DataIngestionLib.Contracts;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DataIngestionLib.DocIngestion;

public sealed class SqlTableMaint
{
    private const int DefaultBatchSize = 25;
    private const int SqlCommandTimeoutSeconds = 120;

    private readonly Func<string> _connectionStringProvider;
    private readonly IChunkMetadataGenerator _metadataGenerator;
    private readonly ILogger<SqlTableMaint> _logger;
    private readonly int _batchSize;

    public SqlTableMaint(IAppSettings appSettings, IChunkMetadataGenerator metadataGenerator, ILogger<SqlTableMaint> logger)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(metadataGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionStringProvider = () => ResolveConnectionString(appSettings);
        _metadataGenerator = metadataGenerator;
        _logger = logger;
        _batchSize = DefaultBatchSize;
    }

    internal SqlTableMaint(Func<string> connectionStringProvider, IChunkMetadataGenerator metadataGenerator, ILogger<SqlTableMaint> logger, int batchSize = DefaultBatchSize)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(metadataGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be greater than zero.");
        }

        _connectionStringProvider = connectionStringProvider;
        _metadataGenerator = metadataGenerator;
        _logger = logger;
        _batchSize = batchSize;
    }

    public Task UpdateMetadataAsync()
    {
        return UpdateMetadataAsync(CancellationToken.None);
    }

    public async Task<MetadataUpdateResult> UpdateMetadataAsync(CancellationToken cancellationToken)
    {
        int updatedCount = 0;
        int failedCount = 0;
        int batchNumber = 0;

        _logger.LogInformation("Starting markdown chunk metadata update with batch size {BatchSize}.", _batchSize);

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<PendingChunk> pendingChunks = await FetchPendingChunkBatchAsync(cancellationToken).ConfigureAwait(false);
                if (pendingChunks.Count == 0)
                {
                    MetadataUpdateResult completedResult = new(updatedCount, failedCount);
                    _logger.LogInformation("Markdown chunk metadata update completed. Updated {UpdatedCount} chunk(s); {FailedCount} failed.", completedResult.UpdatedCount, completedResult.FailedCount);
                    return completedResult;
                }

                batchNumber++;
                _logger.LogInformation("Processing metadata batch {BatchNumber} containing {ChunkCount} chunk(s).", batchNumber, pendingChunks.Count);

                MetadataUpdateResult batchResult = await ProcessPendingChunksAsync(
                    pendingChunks,
                    PersistChunkMetadataAsync,
                    cancellationToken).ConfigureAwait(false);

                updatedCount += batchResult.UpdatedCount;
                failedCount += batchResult.FailedCount;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Markdown chunk metadata update was canceled after updating {UpdatedCount} chunk(s); {FailedCount} failed.", updatedCount, failedCount);
            throw;
        }
    }

    internal async Task<MetadataUpdateResult> ProcessPendingChunksAsync(
        IReadOnlyList<PendingChunk> pendingChunks,
        Func<PendingChunk, GeneratedChunkMetadata, CancellationToken, Task> persistAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pendingChunks);
        ArgumentNullException.ThrowIfNull(persistAsync);

        int updatedCount = 0;
        int failedCount = 0;

        foreach (PendingChunk pendingChunk in pendingChunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                GeneratedChunkMetadata metadata = await _metadataGenerator.GenerateAsync(
                    pendingChunk.Content,
                    pendingChunk.RequiresKeywords,
                    pendingChunk.RequiresSummary,
                    cancellationToken).ConfigureAwait(false);

                await persistAsync(pendingChunk, metadata, cancellationToken).ConfigureAwait(false);
                updatedCount++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedCount++;
                _logger.LogWarning(
                    exception,
                    "Failed to update metadata for chunk {ChunkId} in document {DocumentId} at chunk index {ChunkIndex}. Continuing with the remaining batch.",
                    pendingChunk.ChunkId,
                    pendingChunk.DocumentId,
                    pendingChunk.ChunkIndex);
            }
        }

        return new MetadataUpdateResult(updatedCount, failedCount);
    }

    private async Task<IReadOnlyList<PendingChunk>> FetchPendingChunkBatchAsync(CancellationToken cancellationToken)
    {
        const string query = """
                             SELECT TOP (@batchSize)
                                 [chunk_id],
                                 [doc_id],
                                 [chunk_index],
                                 [content],
                                 [keywords],
                                 [summary]
                             FROM [dbo].[md_document_chunks]
                             WHERE NULLIF(LTRIM(RTRIM([keywords])), N'') IS NULL
                                OR NULLIF(LTRIM(RTRIM([summary])), N'') IS NULL
                             ORDER BY [created_at] ASC, [doc_id] ASC, [chunk_index] ASC;
                             """;

        List<PendingChunk> pendingChunks = [];

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(query, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = SqlCommandTimeoutSeconds
        };

        _ = command.Parameters.Add("@batchSize", SqlDbType.Int);
        command.Parameters["@batchSize"].Value = _batchSize;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            Guid chunkId = reader.GetGuid(reader.GetOrdinal("chunk_id"));
            Guid documentId = reader.GetGuid(reader.GetOrdinal("doc_id"));
            int chunkIndex = reader.GetInt32(reader.GetOrdinal("chunk_index"));
            string content = reader.GetString(reader.GetOrdinal("content"));
            bool requiresKeywords = IsNullOrWhiteSpace(reader, "keywords");
            bool requiresSummary = IsNullOrWhiteSpace(reader, "summary");

            pendingChunks.Add(new PendingChunk(chunkId, documentId, chunkIndex, content, requiresKeywords, requiresSummary));
        }

        return pendingChunks;
    }

    private async Task PersistChunkMetadataAsync(PendingChunk pendingChunk, GeneratedChunkMetadata metadata, CancellationToken cancellationToken)
    {
        const string updateStatement = """
                                       UPDATE [dbo].[md_document_chunks]
                                       SET
                                           [keywords] = CASE
                                               WHEN NULLIF(LTRIM(RTRIM([keywords])), N'') IS NULL THEN COALESCE(@keywords, [keywords])
                                               ELSE [keywords]
                                           END,
                                           [summary] = CASE
                                               WHEN NULLIF(LTRIM(RTRIM([summary])), N'') IS NULL THEN COALESCE(@summary, [summary])
                                               ELSE [summary]
                                           END
                                       WHERE [chunk_id] = @chunk_id;
                                       """;

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(updateStatement, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = SqlCommandTimeoutSeconds
        };

        _ = command.Parameters.Add("@chunk_id", SqlDbType.UniqueIdentifier);
        _ = command.Parameters.Add("@keywords", SqlDbType.NVarChar, -1);
        _ = command.Parameters.Add("@summary", SqlDbType.NVarChar, 4000);

        command.Parameters["@chunk_id"].Value = pendingChunk.ChunkId;
        command.Parameters["@keywords"].Value = metadata.Keywords is null ? DBNull.Value : metadata.Keywords;
        command.Parameters["@summary"].Value = metadata.Summary is null ? DBNull.Value : metadata.Summary;

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rowsAffected != 1)
        {
            throw new InvalidOperationException($"Expected to update one chunk row for '{pendingChunk.ChunkId}', but updated {rowsAffected} rows.");
        }
    }

    private async Task<SqlConnection> OpenRemoteRagConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = _connectionStringProvider();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Remote RAG connection string is not configured.");
        }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static bool IsNullOrWhiteSpace(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return true;
        }

        return string.IsNullOrWhiteSpace(reader.GetString(ordinal));
    }

    private static string ResolveConnectionString(IAppSettings appSettings)
    {
        if (!string.IsNullOrWhiteSpace(appSettings.RemoteRAGConnectionString))
        {
            return appSettings.RemoteRAGConnectionString;
        }

        return Environment.GetEnvironmentVariable("REMOTE_RAG") ?? string.Empty;
    }

    internal readonly record struct PendingChunk(
        Guid ChunkId,
        Guid DocumentId,
        int ChunkIndex,
        string Content,
        bool RequiresKeywords,
        bool RequiresSummary);
}

public readonly record struct MetadataUpdateResult(int UpdatedCount, int FailedCount);
