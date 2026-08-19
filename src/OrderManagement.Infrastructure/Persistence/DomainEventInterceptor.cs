using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.outbox;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Persistence;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IEventTypeRegistry _eventRegistry;

    public DomainEventInterceptor(IEventTypeRegistry eventRegistry)
    {
        _eventRegistry = eventRegistry;
    }

    // Chạy TRƯỚC khi SaveChanges thực sự ghi xuống DB
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Chạy SAU khi SaveChanges thành công
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void AddDomainEventsToOutbox(DbContext? context)
    {
        if (context is null) return;

        var entitiesWithEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = domainEvent.EventId,
                    Type = _eventRegistry.GetEventName(domainEvent.GetType()),
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = domainEvent.OccurredOnUtc,
                    Status = "Pending",
                    RetryCount = 0
                };

                context.Set<OutboxMessage>().Add(outboxMessage);
            }
        }
    }

    private static void ClearDomainEvents(DbContext? context)
    {
        if (context is null) return;

        var entitiesWithEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity);

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
    }
}
