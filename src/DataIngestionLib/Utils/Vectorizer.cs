// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Vectorizer.cs
// Author: Kyle L. Crowder
// Build Num: 133622



using DataIngestionLib.Agents;
using DataIngestionLib.Providers;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Utils;





// mini utility class to vectorize text data for use in ML models, etc.
internal class Vectorizer
{

    public static async Task<SqlChatHistoryProvider.HybridSearch> ToVector(SqlChatHistoryProvider.HybridSearch hybridSearch)
    {
        var generator = AgentFactory.GetEmbeddingClient();


        var aFloat = await generator.GenerateAsync(hybridSearch.SearchPhrase).ConfigureAwait(false);

        hybridSearch.VectorQuery = string.Join(",", aFloat.Vector.ToArray());
        return hybridSearch;
    }
}