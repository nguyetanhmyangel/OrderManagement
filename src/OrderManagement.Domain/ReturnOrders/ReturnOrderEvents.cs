using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.ReturnOrders;

public sealed record ReturnOrderRequestedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    Guid CustomerId) : IDomainEvent;

public sealed record ReturnOrderApprovedEvent(
    Guid ReturnOrderId,
    Guid OrderId) : IDomainEvent;

public sealed record ReturnOrderRejectedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    string Reason) : IDomainEvent;

public sealed record ReturnOrderReceivedEvent(
    Guid ReturnOrderId,
    Guid OrderId) : IDomainEvent;

public sealed record ReturnOrderRefundedEvent(
    Guid ReturnOrderId,
    Guid OrderId,
    decimal TotalRefundAmount,
    string Currency) : IDomainEvent;
