using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Customers;

public sealed record CustomerCreatedEvent(
    Guid CustomerId,
    string Email,
    string FullName) : IDomainEvent;

public sealed record CustomerTierUpgradedEvent(
    Guid CustomerId,
    CustomerTier OldTier,
    CustomerTier NewTier) : IDomainEvent;

public sealed record CustomerDeactivatedEvent(
    Guid CustomerId) : IDomainEvent;

public sealed record CustomerActivatedEvent(
    Guid CustomerId) : IDomainEvent;

public sealed record CustomerLoyaltyPointsChangedEvent(
    Guid CustomerId,
    int PointsDelta,
    int NewBalance) : IDomainEvent;
