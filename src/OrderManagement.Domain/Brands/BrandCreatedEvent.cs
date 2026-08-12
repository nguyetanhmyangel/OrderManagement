using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Brands;

public sealed record BrandCreatedEvent(
    Guid BrandId,
    string Name,
    Guid EventId,
    DateTime OccurredOnUtc) : IDomainEvent
{
    // Constructor tiện lợi cho Domain: Tự gán EventId & UtcNow nếu không truyền vào
    public BrandCreatedEvent(Guid brandId, string name)
        : this(brandId, name, Guid.NewGuid(), DateTime.UtcNow) { }
}
