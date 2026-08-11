using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.ReturnOrders;

/// <summary>
/// Entity owned by ReturnOrder Aggregate.
/// References OrderItem / Product by Id + snapshot.
/// </summary>
public sealed class ReturnItem : Entity<Guid>
{
    public Guid ReturnOrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public string? Reason { get; private set; }
    public bool IsReceived { get; private set; }
    public string? ConditionNote { get; private set; }

    public Money RefundAmount => UnitPrice.Multiply(Quantity);

    private ReturnItem() { }

    private ReturnItem(Guid id, Guid returnOrderId, Guid orderItemId, Guid productId,
        string productName, int quantity, Money unitPrice,
        string? reason, bool isReceived) : base(id)
    {
        ReturnOrderId = returnOrderId;
        OrderItemId = orderItemId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Reason = reason;
        IsReceived = isReceived;
    }

    internal static ReturnItem Create(
        Guid returnOrderId, Guid orderItemId, Guid productId,
        string productName, int quantity, Money unitPrice, string? reason = null)
    {
        if (quantity <= 0)
            throw new DomainException("Số lượng trả phải lớn hơn 0.");
        if (unitPrice.Amount <= 0)
            throw new DomainException("Đơn giá phải lớn hơn 0.");

        return new ReturnItem
        {
            Id = Guid.NewGuid(),
            ReturnOrderId = returnOrderId,
            OrderItemId = orderItemId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Reason = reason,
            IsReceived = false
        };
    }

    internal void MarkAsReceived(string? conditionNote = null)
    {
        IsReceived = true;
        ConditionNote = conditionNote;
    }
}
