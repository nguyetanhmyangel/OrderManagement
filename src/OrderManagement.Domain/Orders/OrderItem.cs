
using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Orders;

/// <summary>
/// Entity owned by Order Aggregate.
/// Product referenced by Id + snapshot only (no Product navigation).
/// </summary>
public sealed class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    // Snapshots at order time
    public string ProductName { get; private set; } = null!;
    public string ProductSku { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public decimal WeightKg { get; private set; }
    public int Quantity { get; private set; }

    public Money Subtotal => UnitPrice.Multiply(Quantity);
    public decimal TotalWeightKg => WeightKg * Quantity;

    private OrderItem() { }

    private OrderItem(Guid id, Guid orderId, Guid productId, string productName, string productSku,
        Money unitPrice, int quantity, decimal weightKg): base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        UnitPrice = unitPrice;
        Quantity = quantity;
        WeightKg = weightKg;
    }
    internal static OrderItem Create(
        Guid orderId, Guid productId, string productName, string productSku,
        Money unitPrice, int quantity, decimal weightKg)
    {
        if (quantity <= 0) throw new DomainException("Số lượng phải lớn hơn 0.");
        if (unitPrice.Amount <= 0) throw new DomainException("Đơn giá phải lớn hơn 0.");
        if (weightKg <= 0) throw new DomainException("Trọng lượng phải lớn hơn 0.");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            ProductSku = productSku,
            UnitPrice = unitPrice,
            Quantity = quantity,
            WeightKg = weightKg
        };
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0) throw new DomainException("Số lượng tăng thêm phải lớn hơn 0.");
        Quantity += amount;
    }

    internal void DecreaseQuantity(int amount)
    {
        if (amount <= 0) throw new DomainException("Số lượng giảm phải lớn hơn 0.");
        if (Quantity - amount <= 0)
            throw new DomainException("Số lượng sau giảm phải > 0. Hãy xóa item.");
        Quantity -= amount;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new DomainException("Số lượng phải lớn hơn 0.");
        Quantity = newQuantity;
    }
}
