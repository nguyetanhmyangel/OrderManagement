namespace OrderManagement.SharedKernel;

// Interface đánh dấu Domain Event
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
