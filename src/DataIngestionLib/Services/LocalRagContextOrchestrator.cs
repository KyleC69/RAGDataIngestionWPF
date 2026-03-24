// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         LocalRagContextOrchestrator.cs
// Author: Kyle L. Crowder
// Build Num: 133607



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public sealed class LocalRagContextOrchestrator : IRagContextOrchestrator
{
    private readonly IAppSettings _appSettings;
    private readonly IContextCitationFormatter _citationFormatter;
    private readonly IRagQueryExpander _queryExpander;
    private readonly IRagRetrievalService _retrievalService;








    public LocalRagContextOrchestrator(IRagRetrievalService retrievalService, IRagQueryExpander queryExpander, IContextCitationFormatter citationFormatter, IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(retrievalService);
        ArgumentNullException.ThrowIfNull(queryExpander);
        ArgumentNullException.ThrowIfNull(citationFormatter);
        ArgumentNullException.ThrowIfNull(appSettings);

        _retrievalService = retrievalService;
        _queryExpander = queryExpander;
        _citationFormatter = citationFormatter;
        _appSettings = appSettings;
    }








    public async ValueTask<IReadOnlyList<ChatMessage>> BuildContextMessagesAsync(IReadOnlyList<ChatMessage> requestMessages, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        var queries = _queryExpander.Expand(requestMessages);
        if (queries.Count == 0)
        {
            return [];
        }

        List<IReadOnlyList<RagSearchResult>> resultSets = [];
        foreach (RagSearchQuery query in queries)
        {
            var resultSet = await _retrievalService.SearchAsync(query, cancellationToken).ConfigureAwait(false);
            if (resultSet.Count > 0)
            {
                resultSets.Add(resultSet);
            }
        }

        if (resultSets.Count == 0)
        {
            return [];
        }

        var maxCharacters = Math.Max(500, _appSettings.RAGBudget * 4);
        var body = _citationFormatter.FormatSection("Relevant local knowledge", [
                .. Fuse(resultSets).Select(result => new ContextCitation { Title = result.Title, SourceKind = "local-rag", Locator = result.Id.ToString(), Content = result.Summary })
        ], maxCharacters);

        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        return
        [
                new ChatMessage(new ChatRole(AIChatRole.RAGContext.Value), body)
        ];
    }








    internal static IReadOnlyList<RagSearchResult> Fuse(IReadOnlyList<IReadOnlyList<RagSearchResult>> resultSets)
    {
        Dictionary<int, (RagSearchResult Result, double Score)> fused = [];

        foreach (var resultSet in resultSets)
        {
            IReadOnlyList<RagSearchResult> ranked = resultSet.OrderByDescending(result => result.Score).ToArray();

            for (var index = 0; index < ranked.Count; index++)
            {
                RagSearchResult result = ranked[index];
                var contribution = 1.0 / (60 + index + 1);

                if (fused.TryGetValue(result.Id, out (RagSearchResult Result, double Score) existing))
                {
                    var mergedScore = existing.Score + contribution;
                    RagSearchResult mergedResult = result.Score > existing.Result.Score ? result : existing.Result;
                    fused[result.Id] = (mergedResult, mergedScore);
                    continue;
                }

                fused[result.Id] = (result, contribution);
            }
        }

        return fused.Values.OrderByDescending(entry => entry.Score).ThenByDescending(entry => entry.Result.Score).Select(entry => entry.Result).ToArray();
    }
}