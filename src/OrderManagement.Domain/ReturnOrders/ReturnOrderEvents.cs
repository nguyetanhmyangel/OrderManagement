using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.ReturnOrders;

public sealed record ReturnOrderRequestedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    Guid CustomerId) : DomainEvent;

public sealed record ReturnOrderApprovedEvent(
    Guid ReturnOrderId,
    Guid OrderId) : DomainEvent;

public sealed record ReturnOrderRejectedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    string Reason) : DomainEvent;

public sealed record ReturnOrderReceivedEvent(
    Guid ReturnOrderId,
    Guid OrderId) : DomainEvent;

public sealed record ReturnOrderRefundedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    decimal TotalRefundAmount,
    string Currency) : DomainEvent;
