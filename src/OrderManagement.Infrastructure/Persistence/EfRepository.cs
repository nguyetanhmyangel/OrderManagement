using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.Specifications;
using OrderManagement.Infrastructure.Database;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Persistence;


public sealed class EfRepository<TEntity, TId>
    : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(ApplicationDbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await _set.FindAsync(
            [id],
            cancellationToken);
    }

    public async Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await SpecificationEvaluator
            .GetQuery(_set, specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await SpecificationEvaluator
            .GetQuery(_set, specification)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _set.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Remove(entity);
    }
}
