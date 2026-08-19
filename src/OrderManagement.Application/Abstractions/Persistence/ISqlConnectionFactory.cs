using System.Data;
using System.Data.Common;

namespace OrderManagement.Application.Abstractions.Persistence;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();

    Task<DbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default);
}
