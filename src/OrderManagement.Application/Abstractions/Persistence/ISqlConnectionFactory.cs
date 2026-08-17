using System.Data;

namespace OrderManagement.Application.Abstractions.Persistence;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();
}
