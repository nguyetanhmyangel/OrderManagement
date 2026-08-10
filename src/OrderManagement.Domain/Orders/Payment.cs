using OrderManagement.Domain.Orders;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.orders;

/// <summary>
/// Entity owned by Order Aggregate.
/// Does NOT own Refund. Tracks RefundedAmount only.
/// </summary>
public sealed class Payment : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public string? PaymentGateway { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Money RefundedAmount { get; private set; } = null!;

    public Money RemainingRefundable => Amount.Subtract(RefundedAmount);

    private Payment() { }

    private Payment(Guid id, Guid orderId, Money amount, PaymentMethod method, string? paymentGateway) : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        PaymentGateway = paymentGateway;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Create(
        Guid orderId, Money amount, PaymentMethod method, string? paymentGateway = null)
    {
        if (amount.Amount <= 0)
            throw new DomainException("Số tiền thanh toán phải lớn hơn 0.");

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            PaymentGateway = paymentGateway,
            RefundedAmount = Money.ZeroOf(amount.Currency),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsAuthorized(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Chỉ payment Pending mới authorize. Hiện tại: {Status}");
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new DomainException("TransactionId không được để trống.");

        TransactionId = transactionId;
        Status = PaymentStatus.Authorized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCaptured(string? transactionId = null)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.Authorized))
            throw new DomainException($"Không thể capture payment ở trạng thái {Status}.");

        if (!string.IsNullOrWhiteSpace(transactionId))
            TransactionId = transactionId;

        Status = PaymentStatus.Captured;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (Status is PaymentStatus.Captured or PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            throw new DomainException($"Không thể failed payment đã {Status}.");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is PaymentStatus.Captured or PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            throw new DomainException($"Không thể hủy payment đã {Status}.");

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Ghi nhận hoàn tiền — gọi từ handler sau khi Refund completed.</summary>
    public void ApplyRefund(Money amount)
    {
        if (Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded))
            throw new DomainException("Chỉ payment đã capture mới được hoàn tiền.");
        if (amount.Amount <= 0)
            throw new DomainException("Số tiền hoàn phải lớn hơn 0.");
        if (amount.IsGreaterThan(RemainingRefundable))
            throw new DomainException(
                $"Hoàn vượt số còn lại. Còn: {RemainingRefundable}, yêu cầu: {amount}.");

        RefundedAmount = RefundedAmount.Add(amount);
        Status = RefundedAmount.Amount >= Amount.Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;
        UpdatedAt = DateTime.UtcNow;
    }
}
