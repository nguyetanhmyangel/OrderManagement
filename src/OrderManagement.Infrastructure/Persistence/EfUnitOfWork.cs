using OrderManagement.Application.Abstractions.Data;
using OrderManagement.Application.Abstractions.Persistence;

namespace OrderManagement.Infrastructure.Persistence;

public sealed class EfUnitOfWork(IApplicationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // DomainEventInterceptor sẽ tự động:
        // 1. Lấy Domain Events → ghi vào Outbox (cùng transaction)
        // 2. Clear Domain Events sau khi Save thành công
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
