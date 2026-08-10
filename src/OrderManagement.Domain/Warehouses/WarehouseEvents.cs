using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Warehouses;

public sealed record WarehouseCreatedEvent(
    Guid WarehouseId,
    string Code,
    string Name) : IDomainEvent;

public sealed record WarehouseDeactivatedEvent(
    Guid WarehouseId,
    string Code) : IDomainEvent;
