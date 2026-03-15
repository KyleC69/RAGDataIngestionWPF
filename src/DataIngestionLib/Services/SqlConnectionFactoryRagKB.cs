// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





/// <summary>
/// Provides SQL connection for RAG knowledge base.
/// </summary>
public static class SqlConnectionFactoryRagKb
    {
    public static SqlConnection CreateConnection()
        {
        var connectionString = Environment.GetEnvironmentVariable("CONN_STRING2") ?? throw new InvalidOperationException("Environment variable 'CONN_STRING2' is not set.");
        return new SqlConnection(connectionString);
        }
    }