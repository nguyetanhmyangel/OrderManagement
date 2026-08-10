
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Inventories;

public sealed record InventoryStockInEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAfter) : IDomainEvent;

public sealed record InventoryStockOutEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAfter) : IDomainEvent;

public sealed record InventoryReservedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int QuantityAvailableAfter) : IDomainEvent;

public sealed record InventoryReservationReleasedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity) : IDomainEvent;

public sealed record InventoryLowStockEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int QuantityAvailable,
    int ReorderLevel) : IDomainEvent;

public sealed record InventoryAdjustedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int QuantityBefore,
    int QuantityAfter) : IDomainEvent;
