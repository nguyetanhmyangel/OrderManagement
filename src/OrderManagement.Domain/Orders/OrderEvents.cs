using OrderManagement.Domain.Orders;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.orders;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail) : DomainEvent;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemSnapshot> Items) : DomainEvent;

public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId) : DomainEvent;

public sealed record OrderPaidEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    PaymentMethod Method) : DomainEvent;

public sealed record OrderShippedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string TrackingNumber,
    string Carrier,
    DateTime? EstimatedDelivery) : DomainEvent;

public sealed record OrderDeliveredEvent(
    Guid OrderId,
    Guid CustomerId) : DomainEvent;

public sealed record OrderCompletedEvent(
    Guid OrderId,
    Guid CustomerId) : DomainEvent;

public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string Reason) : DomainEvent;

public sealed record OrderItemAddedEvent(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : DomainEvent;

public sealed record OrderVoucherAppliedEvent(
    Guid OrderId,
    Guid VoucherId,
    string VoucherCode,
    decimal DiscountAmount,
    string Currency) : DomainEvent;

/// <summary>
/// Snapshot of an order item for event payload (no entity reference).
/// </summary>
public sealed record OrderItemSnapshot(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
