using System.Collections.Concurrent;
using OrderManagement.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManagement.Application.Abstractions.DomainEvents;
public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents.Count == 0) return;

        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var handlerType = HandlerTypeCache.GetOrAdd(eventType, t => typeof(IDomainEventHandler<>).MakeGenericType(t));

            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod("HandleAsync");
                if (method is not null)
                {
                    await (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                }
            }
        }
    }
}
