using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Customers;

public sealed record CustomerCreatedEvent(
    Guid CustomerId,
    string Email,
    string FullName) : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}

public sealed record CustomerTierUpgradedEvent(
    Guid CustomerId,
    CustomerTier OldTier,
    CustomerTier NewTier) : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}

public sealed record CustomerDeactivatedEvent(
    Guid CustomerId) : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}

public sealed record CustomerActivatedEvent(
    Guid CustomerId) : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}

public sealed record CustomerLoyaltyPointsChangedEvent(
    Guid CustomerId,
    int PointsDelta,
    int NewBalance,
    Guid EventId,
    DateTime OccurredOnUtc) : IDomainEvent
{
    // Constructor tiện lợi cho Domain: Tự gán EventId & UtcNow nếu không truyền vào
    public CustomerLoyaltyPointsChangedEvent(Guid customerId, int pointsDelta, int newBalance)
        : this(customerId, pointsDelta, newBalance, Guid.NewGuid(), DateTime.UtcNow) { }
}
