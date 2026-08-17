using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Customers;

public sealed record CustomerCreatedEvent(
    Guid CustomerId,
    string Email,
    string FullName) : DomainEvent;

public sealed record CustomerTierUpgradedEvent(
    Guid CustomerId,
    CustomerTier OldTier,
    CustomerTier NewTier) : DomainEvent;

public sealed record CustomerDeactivatedEvent(
    Guid CustomerId) : DomainEvent;

public sealed record CustomerActivatedEvent(
    Guid CustomerId) : DomainEvent;

public sealed record CustomerLoyaltyPointsChangedEvent(
    Guid CustomerId,
    int PointsDelta,
    int NewBalance) : DomainEvent;
