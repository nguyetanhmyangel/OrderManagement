using OrderManagement.SharedKernel;

namespace OrderManagement.Application.Abstractions.DomainEvents;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default);
}
