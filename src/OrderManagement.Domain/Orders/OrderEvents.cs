using OrderManagement.Domain.Orders;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.orders;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail) : IDomainEvent;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemSnapshot> Items) : IDomainEvent;

public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId) : IDomainEvent;

public sealed record OrderPaidEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    PaymentMethod Method) : IDomainEvent;

public sealed record OrderShippedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string TrackingNumber,
    string Carrier,
    DateTime? EstimatedDelivery) : IDomainEvent;

public sealed record OrderDeliveredEvent(
    Guid OrderId,
    Guid CustomerId) : IDomainEvent;

public sealed record OrderCompletedEvent(
    Guid OrderId,
    Guid CustomerId) : IDomainEvent;

public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string Reason) : IDomainEvent;

public sealed record OrderItemAddedEvent(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : IDomainEvent;

public sealed record OrderVoucherAppliedEvent(
    Guid OrderId,
    Guid VoucherId,
    string VoucherCode,
    decimal DiscountAmount,
    string Currency) : IDomainEvent;

/// <summary>
/// Snapshot of an order item for event payload (no entity reference).
/// </summary>
public sealed record OrderItemSnapshot(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
