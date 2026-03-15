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
/// Is this necessary?
/// </summary>
public interface ISqlChatHistoryConnectionFactory
    {
    ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
    }





public sealed class SqlChatHistoryConnectionFactory : ISqlChatHistoryConnectionFactory
    {

    public async ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
        var connectionString = Environment.GetEnvironmentVariable("CHAT_HISTORY") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
            {
            throw new InvalidOperationException("Chat history connection string is not configured.");
            }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
        }
    }