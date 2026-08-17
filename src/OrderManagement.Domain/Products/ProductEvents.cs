using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Products;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Sku,
    string Name,
    decimal Price,
    string Currency) : DomainEvent;

public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    string Currency) : DomainEvent;

public sealed record ProductDeactivatedEvent(
    Guid ProductId) : DomainEvent;

public sealed record ProductActivatedEvent(
    Guid ProductId) : DomainEvent;

public sealed record ProductImageAddedEvent(
    Guid ProductId,
    Guid ImageId,
    string Url,
    bool IsPrimary) : DomainEvent;
