using OrderManagement.Domain.Inventories;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Inventories;

/// <summary>
/// Aggregate Root — stock per Product + Warehouse.
/// References Product / Warehouse by Id only.
/// Owns: InventoryTransaction.
/// </summary>
public sealed class Inventory : Entity<Guid>, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public int ReorderLevel { get; private set; }
    public int ReorderQuantity { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    public bool IsLowStock => QuantityAvailable <= ReorderLevel;
    public bool IsOutOfStock => QuantityAvailable <= 0;

    private readonly List<InventoryTransaction> _transactions = [];
    public IReadOnlyList<InventoryTransaction> Transactions => _transactions.AsReadOnly();

    private Inventory() { }

    private Inventory(Guid id, Guid productId, Guid warehouseId,
        int initialQuantity, int reorderLevel, int reorderQuantity,DateTime updateAt)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        QuantityOnHand = initialQuantity;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        UpdatedAt = updateAt;
    }

    public static Inventory Create(
        Guid productId, Guid warehouseId,
        int initialQuantity = 0, int reorderLevel = 10, int reorderQuantity = 50)
    {
        if (initialQuantity < 0)
            throw new DomainException(InventoryErrors.NegativeQuantity.Description);
        if (reorderLevel < 0)
            throw new DomainException(InventoryErrors.InvalidReorderSettings.Description);

        return new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityOnHand = initialQuantity,
            QuantityReserved = 0,
            ReorderLevel = reorderLevel,
            ReorderQuantity = reorderQuantity,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void StockIn(int quantity, string? note = null, Guid? performedBy = null)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.InvalidQuantity.Description);

        var before = QuantityOnHand;
        QuantityOnHand += quantity;
        UpdatedAt = DateTime.UtcNow;
        AddTx(InventoryTransactionType.StockIn, quantity, before, QuantityOnHand, note, performedBy);
        RaiseDomainEvent(new InventoryStockInEvent(Id, ProductId, WarehouseId, quantity, QuantityOnHand));
    }

    public void StockOut(int quantity, string? note = null, Guid? performedBy = null)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.InvalidQuantity.Description);
        if (QuantityOnHand < quantity)
            throw new DomainException(InventoryErrors.InsufficientStock(QuantityOnHand, quantity).Description);

        var before = QuantityOnHand;
        QuantityOnHand -= quantity;
        UpdatedAt = DateTime.UtcNow;
        AddTx(InventoryTransactionType.StockOut, quantity, before, QuantityOnHand, note, performedBy);
        RaiseDomainEvent(new InventoryStockOutEvent(Id, ProductId, WarehouseId, quantity, QuantityOnHand));
        CheckLowStock();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.InvalidQuantity.Description);
        if (QuantityAvailable < quantity)
            throw new DomainException(InventoryErrors.InsufficientAvailable(QuantityAvailable, quantity).Description);

        QuantityReserved += quantity;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new InventoryReservedEvent(Id, ProductId, WarehouseId, quantity, QuantityAvailable));
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.InvalidQuantity.Description);
        if (QuantityReserved < quantity)
            throw new DomainException(InventoryErrors.InsufficientReserved(QuantityReserved, quantity).Description);

        QuantityReserved -= quantity;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new InventoryReservationReleasedEvent(Id, ProductId, WarehouseId, quantity));
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.InvalidQuantity.Description);
        if (QuantityReserved < quantity)
            throw new DomainException(InventoryErrors.InsufficientReserved(QuantityReserved, quantity).Description);

        var before = QuantityOnHand;
        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        UpdatedAt = DateTime.UtcNow;
        AddTx(InventoryTransactionType.StockOut, quantity, before, QuantityOnHand, "Confirm reservation");
        RaiseDomainEvent(new InventoryStockOutEvent(Id, ProductId, WarehouseId, quantity, QuantityOnHand));
        CheckLowStock();
    }

    public void Adjust(int newQty, string? note = null, Guid? performedBy = null)
    {
        if (newQty < 0)
            throw new DomainException(InventoryErrors.NegativeQuantity.Description);
        if (newQty < QuantityReserved)
            throw new DomainException(InventoryErrors.AdjustBelowReserved(QuantityReserved).Description);

        var before = QuantityOnHand;
        var delta = Math.Abs(newQty - before);
        QuantityOnHand = newQty;
        UpdatedAt = DateTime.UtcNow;
        if (delta > 0)
            AddTx(InventoryTransactionType.Adjustment, delta, before, QuantityOnHand, note, performedBy);
        RaiseDomainEvent(new InventoryAdjustedEvent(Id, ProductId, WarehouseId, before, QuantityOnHand));
        CheckLowStock();
    }

    public void UpdateReorderSettings(int reorderLevel, int reorderQuantity)
    {
        if (reorderLevel < 0 || reorderQuantity <= 0)
            throw new DomainException(InventoryErrors.InvalidReorderSettings.Description);
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    private void AddTx(
        InventoryTransactionType type, int qty, int before, int after,
        string? note = null, Guid? performedBy = null,
        string? refType = null, Guid? refId = null)
    {
        _transactions.Add(InventoryTransaction.Create(
            Id, ProductId, WarehouseId, type, qty, before, after,
            refType, refId, note, performedBy));
    }

    private void CheckLowStock()
    {
        if (IsLowStock)
            RaiseDomainEvent(new InventoryLowStockEvent(Id, ProductId, WarehouseId, QuantityAvailable, ReorderLevel));
    }
}
