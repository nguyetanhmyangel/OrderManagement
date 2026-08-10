using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Brands;

public sealed record BrandCreatedEvent(
    Guid BrandId,
    string Name) : IDomainEvent;
