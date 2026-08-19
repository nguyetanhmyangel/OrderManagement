using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.SharedKernel;

namespace OrderManagement.Application.Abstractions.DomainEvents;

public sealed class DomainEventsDispatcher(
    IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();

        var handlerType = HandlerTypeCache.GetOrAdd(
            eventType,
            static type => typeof(IDomainEventHandler<>)
                .MakeGenericType(type));

        var handlers = serviceProvider.GetServices(handlerType);

        var handleMethod = handlerType.GetMethod(
            nameof(IDomainEventHandler<IDomainEvent>.Handle));

        if (handleMethod is null)
        {
            throw new InvalidOperationException(
                $"Không tìm thấy Handle method cho handler type '{handlerType}'.");
        }

        foreach (var handler in handlers)
        {
            if (handler is null)
                continue;

            await (Task)handleMethod.Invoke(
                handler,
                [domainEvent, cancellationToken])!;
        }
    }
}
