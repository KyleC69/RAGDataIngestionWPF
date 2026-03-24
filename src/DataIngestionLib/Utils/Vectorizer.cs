// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//

using System;
using System.Collections.Generic;
using System.Text;

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

      hybridSearch.VectorQuery= string.Join(",", aFloat.Vector.ToArray());
        return hybridSearch;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
}
