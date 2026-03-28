// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class LocalRagContextSource
{
    private readonly ILogger<AIContextRAGInjector> _logger;


    public LocalRagContextSource(ILogger<AIContextRAGInjector> logger)
    {
        _logger = logger;
    }

    public string[] SearchSqlRagSource(string message)
    {
        _logger.LogInformation("Searching SQL RAG source for message: {Message}", message);
        SqlConnection connection = new(Environment.GetEnvironmentVariable("REMOTE_RAG"));

        List<string> results = new();
        try
        {
            SqlCommand cmd = new($"EXEC sp_LearnDocs_Search_Vector @QueryText = {message}");
            cmd.Connection = connection;
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(reader.GetString(0)); // Assuming the first column contains the result string
            }


            _logger.LogInformation("Found {Count} results.", results.Count);
            return results.ToArray();

        }
        catch (Exception)
        {
            _logger.LogError("An error occured searching SQL RAG source.");

        }
        finally
        {
            connection.Close();

        }
        return new string[0];
    }



}