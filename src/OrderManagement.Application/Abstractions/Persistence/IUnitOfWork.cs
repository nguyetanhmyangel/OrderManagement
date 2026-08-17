using System.Data;

namespace OrderManagement.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    void TrackEntity(object entity);
    void EnqueueCommand(Func<IDbConnection, IDbTransaction, Task> sqlOperation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
