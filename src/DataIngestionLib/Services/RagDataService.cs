// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 140813



using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.RAGModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;




namespace DataIngestionLib.Services;





public class RagDataService(ILogger<RagDataService> logger) : IRagRetrievalService
{
    private readonly ILogger<RagDataService> _logger = logger;








    public async ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(RagSearchQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return [];
        }

        var commandText = query.Mode == RagSearchMode.FullText ? "EXEC sp_Search_FullText @query, @topK" : "EXEC sp_Search_hybrid @query, @topK";

        List<RagSearchResult> results = [];
        SqlConnection conn = SqlConnectionFactoryRagKb.CreateConnection();
        await using ConfiguredAsyncDisposable conn1 = conn.ConfigureAwait(false);
        await using SqlCommand cmd = new(commandText, conn);
        _ = cmd.Parameters.AddWithValue("@query", query.Query);
        _ = cmd.Parameters.AddWithValue("@topK", query.TopK);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await using ConfiguredAsyncDisposable reader1 = reader.ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            results.Add(new RagSearchResult(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), reader.GetDouble(4)));

        return results;
    }








    public static string FullTextSearch(string query, int topK = 5)
    {
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKb.CreateConnection();

        using SqlCommand cmd = new("EXEC sp_Search_FullText @query, @topK", conn);
        _ = cmd.Parameters.AddWithValue("@query", query);
        _ = cmd.Parameters.AddWithValue("@topK", topK);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(new FullTextResults
            {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Summary = reader.GetString(2),
                    Keywords = reader.GetString(3).Split(','),
                    Score = reader.GetDouble(4)
            });

        return JsonConvert.SerializeObject(results);
    }








    public ObservableCollection<RemoteRag> GetRagDataEntries()
    {
        ObservableCollection<RemoteRag> rags = [];

        try
        {
            using RAGContext context = new();
            context.RemoteRags.Load();
            rags = context.RemoteRags.Local.ToObservableCollection();
        }
        catch (Exception ex)
        {
            _logger.LogErrorFetchingRAGDataEntriesMessage(ex.Message);
        }

        return rags;
    }








    public static string HybridSearch(string query, int topK = 5)
    {
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKb.CreateConnection();

        using SqlCommand cmd = new("EXEC sp_Search_hybrid @query, @topK", conn);
        _ = cmd.Parameters.AddWithValue("@query", query);
        _ = cmd.Parameters.AddWithValue("@topK", topK);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(new FullTextResults
            {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Summary = reader.GetString(2),
                    Keywords = reader.GetString(3).Split(','),
                    Score = reader.GetDouble(4)
            });

        return JsonConvert.SerializeObject(results);
    }
}





public sealed class FullTextResults
{
    public int Id { get; init; }
    public string[] Keywords { get; init; } = [];
    public double Score { get; init; }
    public required string Summary { get; init; }
    public required string Title { get; init; }
}