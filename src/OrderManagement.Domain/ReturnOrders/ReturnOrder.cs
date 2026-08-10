using OrderManagement.Domain.Customers;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ReturnOrders;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.ReturnOrders;

/// <summary>
/// Aggregate Root — Return / refund request.
/// Owns: ReturnItem, Refund.
/// References Order / Customer / Payment by Id only.
/// </summary>
public sealed class ReturnOrder : Entity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ReturnStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public string? RejectReason { get; private set; }
    public Money TotalRefundAmount { get; private set; } = null!;
    public DateTime RequestedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ReturnItem> _items = [];
    public IReadOnlyList<ReturnItem> Items => _items.AsReadOnly();

    private readonly List<Refund> _refunds = [];
    public IReadOnlyList<Refund> Refunds => _refunds.AsReadOnly();

    private ReturnOrder() { }

    public static ReturnOrder Create(Guid orderId, Guid customerId, string? reason = null)
    {
        var ro = new ReturnOrder
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Status = ReturnStatus.Requested,
            Reason = reason,
            TotalRefundAmount = Money.Zero,
            RequestedAt = DateTime.UtcNow
        };

        ro.RaiseDomainEvent(new ReturnOrderRequestedEvent(ro.Id, orderId, customerId));
        return ro;
    }

    public void AddItem(
        Guid orderItemId, Guid productId, string productName,
        int quantity, Money unitPrice, string? reason = null)
    {
        EnsureModifiable();
        if (_items.Any(i => i.OrderItemId == orderItemId))
            throw new DomainException(ReturnOrderErrors.ItemAlreadyAdded.Description);

        _items.Add(ReturnItem.Create(Id, orderItemId, productId, productName, quantity, unitPrice, reason));
        RecalculateTotal();
    }

    public void Approve()
    {
        if (Status != ReturnStatus.Requested)
            throw new DomainException(ReturnOrderErrors.CannotApprove(Status).Description);
        if (!_items.Any())
            throw new DomainException(ReturnOrderErrors.EmptyItems.Description);

        Status = ReturnStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ReturnOrderApprovedEvent(Id, OrderId));
    }

    public void Reject(string rejectReason)
    {
        if (Status != ReturnStatus.Requested)
            throw new DomainException(ReturnOrderErrors.CannotReject(Status).Description);
        if (string.IsNullOrWhiteSpace(rejectReason))
            throw new DomainException(ReturnOrderErrors.RejectReasonRequired.Description);

        Status = ReturnStatus.Rejected;
        RejectReason = rejectReason;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ReturnOrderRejectedEvent(Id, OrderId, rejectReason));
    }

    public void MarkAsReceived()
    {
        if (Status != ReturnStatus.Approved)
            throw new DomainException(ReturnOrderErrors.CannotMarkReceived(Status).Description);

        foreach (var item in _items) item.MarkAsReceived();
        Status = ReturnStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ReturnOrderReceivedEvent(Id, OrderId));
    }

    /// <summary>Tạo Refund thuộc ReturnOrder; PaymentId chỉ là reference.</summary>
    public Refund CreateRefund(Guid paymentId, Money amount, string? reason = null)
    {
        if (Status is not (ReturnStatus.Received or ReturnStatus.Approved))
            throw new DomainException("Chỉ return Approved/Received mới tạo refund.");

        var refund = Refund.Create(Id, paymentId, OrderId, amount, reason);
        _refunds.Add(refund);
        return refund;
    }

    public void MarkAsRefunded()
    {
        if (Status != ReturnStatus.Received)
            throw new DomainException(ReturnOrderErrors.CannotMarkRefunded(Status).Description);

        Status = ReturnStatus.Refunded;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ReturnOrderRefundedEvent(
            Id, OrderId, TotalRefundAmount.Amount, TotalRefundAmount.Currency));
    }

    public void Close()
    {
        if (Status is not (ReturnStatus.Refunded or ReturnStatus.Rejected))
            throw new DomainException(ReturnOrderErrors.CannotClose(Status).Description);
        Status = ReturnStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalRefundAmount = _items.Aggregate(Money.Zero, (sum, i) => sum.Add(i.RefundAmount));
    }

    private void EnsureModifiable()
    {
        if (Status is not ReturnStatus.Requested)
            throw new DomainException(ReturnOrderErrors.NotModifiable(Status).Description);
    }
}
