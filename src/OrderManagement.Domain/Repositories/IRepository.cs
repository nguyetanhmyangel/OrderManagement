using OrderManagement.Domain.Specifications;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Repositories;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
