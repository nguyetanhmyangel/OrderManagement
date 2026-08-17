using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Promotions;

public sealed record PromotionCreatedEvent(
    Guid PromotionId,
    string Name,
    DateTime ValidFrom,
    DateTime ValidTo) : DomainEvent;
