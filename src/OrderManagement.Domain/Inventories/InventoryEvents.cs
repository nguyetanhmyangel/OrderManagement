
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Inventories;

public sealed record InventoryStockInEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAfter) : DomainEvent;

public sealed record InventoryStockOutEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAfter) : DomainEvent;

public sealed record InventoryReservedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAvailableAfter) : DomainEvent;

public sealed record InventoryReservationReleasedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity) : DomainEvent;

public sealed record InventoryLowStockEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int QuantityAvailable,
    int ReorderLevel) : DomainEvent;

public sealed record InventoryAdjustedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int QuantityBefore,
    int QuantityAfter) : DomainEvent;
