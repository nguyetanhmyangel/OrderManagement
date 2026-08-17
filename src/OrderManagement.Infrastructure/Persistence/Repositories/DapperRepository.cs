using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Domain.Repositories;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public abstract class DapperRepository<TEntity, TId>(
    ISqlConnectionFactory connectionFactory,
    IUnitOfWork unitOfWork) : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected readonly ISqlConnectionFactory ConnectionFactory = connectionFactory;
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;

    public abstract Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    public abstract Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    public abstract void Update(TEntity entity);
    public abstract void Remove(TEntity entity);

    protected void TrackEvents(TEntity entity) => UnitOfWork.TrackEntity(entity);
}
