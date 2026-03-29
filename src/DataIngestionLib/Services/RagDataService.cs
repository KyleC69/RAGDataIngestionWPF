// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using DataIngestionLib.EFModels;

using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





public class RagDataService(ILogger<RagDataService> logger, IAIRemoteRagContextProcedures context)
{
    private readonly ILogger<RagDataService> _logger = logger;
    private readonly IAIRemoteRagContextProcedures _context = context;





    public async ValueTask<List<Search_FullTextResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        List<Search_FullTextResult> results = await _context.Search_FullTextAsync(query, 5, cancellationToken: cancellationToken);



        return results;
    }








    public async ValueTask<List<sp_LearnDocs_Search_VectorResult>> FullTextSearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        List<sp_LearnDocs_Search_VectorResult> results = await _context.sp_LearnDocs_Search_VectorAsync(query, topK, cancellationToken: cancellationToken);
        return results;



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