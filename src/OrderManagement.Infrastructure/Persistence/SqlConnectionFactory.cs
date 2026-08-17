using System.Data;
using Npgsql;
using OrderManagement.Application.Abstractions.Persistence;

namespace OrderManagement.Infrastructure.Persistence;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection GetOpenConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}

// Add Microsoft.Data.SqlClient if use sql server
// public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
// {
//     public IDbConnection GetOpenConnection()
//     {
//         var connection = new SqlConnection(connectionString);
//         connection.Open();
//         return connection;
//     }
// }
