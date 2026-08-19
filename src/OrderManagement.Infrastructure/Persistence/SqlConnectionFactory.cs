using System.Data;
using System.Data.Common;
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

    public async Task<DbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync(
            cancellationToken);

        return connection;
    }
}

