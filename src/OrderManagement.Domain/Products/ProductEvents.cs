using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Products;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Sku,
    string Name,
    decimal Price,
    string Currency) : IDomainEvent;

public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    string Currency) : IDomainEvent;

public sealed record ProductDeactivatedEvent(
    Guid ProductId) : IDomainEvent;

public sealed record ProductActivatedEvent(
    Guid ProductId) : IDomainEvent;

public sealed record ProductImageAddedEvent(
    Guid ProductId,
    Guid ImageId,
    string Url,
    bool IsPrimary) : IDomainEvent;
