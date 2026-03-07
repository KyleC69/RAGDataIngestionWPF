// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         Class1.cs
//   Author: Kyle L. Crowder



using Microsoft.Data.SqlClient;
using Microsoft.Extensions.VectorData;




namespace DataIngestionLib;





public interface ISqlVectorStore
{
    void SaveChatHistory(string userId, string message, DateTime timestamp);


    IEnumerable<(string Message, DateTime Timestamp)> GetChatHistory(string userId);
}




//SQL backed vector store implementation for chat history.
public class SqlVectorStore : VectorStore, ISqlVectorStore
{
    private readonly string _connectionString;
    private readonly string _tableName = "AIChatHistory";
    public SqlVectorStore(string? connectionString)
    {
        _connectionString = connectionString ?? Environment.GetEnvironmentVariable("CHAT_HISTORY") ?? throw new ArgumentNullException(nameof(connectionString), "Connection string must be provided.");
    }
    public void SaveChatHistory(string userId, string message, DateTime timestamp)
    {
        using (SqlConnection connection = new(_connectionString))
        {
            connection.Open();
            string query = $"INSERT INTO {_tableName} (UserId, Message, Timestamp) VALUES (@UserId, @Message, @Timestamp)";
            using (SqlCommand command = new(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@Timestamp", timestamp);
                command.ExecuteNonQuery();
            }
        }
    }
    public IEnumerable<(string Message, DateTime Timestamp)> GetChatHistory(string userId)
    {
        List<(string Message, DateTime Timestamp)> chatHistory = [];
        using (SqlConnection connection = new(_connectionString))
        {
            connection.Open();
            string query = $"SELECT Message, Timestamp FROM {_tableName} WHERE UserId = @UserId ORDER BY Timestamp";
            using (SqlCommand command = new(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        chatHistory.Add((reader.GetString(0), reader.GetDateTime(1)));
                    }
                }
            }
        }

        return chatHistory;
    }

    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name, VectorStoreCollectionDefinition? definition = null)
    {
        throw new NotImplementedException();
    }

    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
    {
        throw new NotImplementedException();
    }

    public override IAsyncEnumerable<string> ListCollectionNamesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }
}
