namespace OrderManagement.SharedKernel;

public abstract record DomainEvent(Guid EventId, DateTime OccurredOnUtc) : IDomainEvent
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}
