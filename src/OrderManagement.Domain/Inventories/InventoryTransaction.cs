
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Inventories;

/// <summary>Entity owned by Inventory Aggregate.</summary>
public sealed class InventoryTransaction : Entity<Guid>
{
    public Guid InventoryId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public InventoryTransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public int QuantityBefore { get; private set; }
    public int QuantityAfter { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? Note { get; private set; }
    public Guid? PerformedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private InventoryTransaction() { }

    private InventoryTransaction(Guid id, Guid inventoryId, Guid productId, Guid warehouseId,
        InventoryTransactionType type, int quantity,
        int quantityBefore, int quantityAfter,
        string? referenceType, Guid? referenceId,
        string? note, Guid? performedBy) : base(id)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        Quantity = quantity;
        QuantityBefore = quantityBefore;
        QuantityAfter = quantityAfter;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Note = note;
        PerformedBy = performedBy;
        CreatedAt = DateTime.UtcNow;
    }

    internal static InventoryTransaction Create(
        Guid inventoryId, Guid productId, Guid warehouseId,
        InventoryTransactionType type, int quantity,
        int quantityBefore, int quantityAfter,
        string? referenceType = null, Guid? referenceId = null,
        string? note = null, Guid? performedBy = null)
    {
        if (quantity <= 0)
            throw new DomainException("Số lượng giao dịch phải lớn hơn 0.");

        return new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryId = inventoryId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Type = type,
            Quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Note = note,
            PerformedBy = performedBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
