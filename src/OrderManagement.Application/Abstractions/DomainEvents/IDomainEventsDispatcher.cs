using OrderManagement.SharedKernel;

namespace OrderManagement.Application.Abstractions.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
