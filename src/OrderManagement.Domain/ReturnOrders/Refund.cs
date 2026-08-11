using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.ReturnOrders;

/// <summary>
/// Entity owned by ReturnOrder Aggregate.
/// Payment / Order referenced by Id only.
/// </summary>
public sealed class Refund : Entity<Guid>
{
    public Guid ReturnOrderId { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public RefundStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Refund() { }

    private Refund(Guid id, Guid returnOrderId, Guid paymentId, Guid orderId,
        Money amount, RefundStatus status, string? reason, DateTime createdAt) : base(id)
    {
        ReturnOrderId = returnOrderId;
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Status = status;
        Reason = reason;
        CreatedAt = createdAt;
    }

    internal static Refund Create(
        Guid returnOrderId, Guid paymentId, Guid orderId,
        Money amount, string? reason = null)
    {
        if (amount.Amount <= 0)
            throw new DomainException("Số tiền hoàn phải lớn hơn 0.");
        if (returnOrderId == Guid.Empty)
            throw new DomainException("ReturnOrderId không hợp lệ.");
        if (paymentId == Guid.Empty)
            throw new DomainException("PaymentId không hợp lệ.");

        return new Refund
        {
            Id = Guid.NewGuid(),
            ReturnOrderId = returnOrderId,
            PaymentId = paymentId,
            OrderId = orderId,
            Amount = amount,
            Status = RefundStatus.Pending,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing(string? transactionId = null)
    {
        if (Status != RefundStatus.Pending)
            throw new DomainException($"Chỉ refund Pending mới Processing. Hiện tại: {Status}");

        Status = RefundStatus.Processing;
        if (!string.IsNullOrWhiteSpace(transactionId))
            TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string? transactionId = null)
    {
        if (Status is not (RefundStatus.Pending or RefundStatus.Processing))
            throw new DomainException($"Không thể complete refund ở trạng thái {Status}.");

        Status = RefundStatus.Completed;
        if (!string.IsNullOrWhiteSpace(transactionId))
            TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (Status == RefundStatus.Completed)
            throw new DomainException("Không thể failed refund đã completed.");

        Status = RefundStatus.Failed;
        Reason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
