using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Orders;

/// <summary>Entity owned by Order Aggregate.</summary>
public sealed class OrderStatusHistory : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Note { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private OrderStatusHistory() { }

    private OrderStatusHistory(Guid id, Guid orderId, OrderStatus from, OrderStatus to,
        string? note, Guid? changedBy) : base(id)
    {
        OrderId = orderId;
        FromStatus = from;
        ToStatus = to;
        Note = note;
        ChangedBy = changedBy;
        ChangedAt = DateTime.UtcNow;
    }

    internal static OrderStatusHistory Create(
        Guid orderId, OrderStatus from, OrderStatus to,
        string? note = null, Guid? changedBy = null)
    {
        return new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FromStatus = from,
            ToStatus = to,
            Note = note,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        };
    }
}
