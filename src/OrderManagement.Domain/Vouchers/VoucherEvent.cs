using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Vouchers;

public sealed record VoucherCreatedEvent(
    Guid VoucherId,
    Guid? PromotionId,
    string Code) : IDomainEvent;

public sealed record VoucherUsedEvent(
    Guid VoucherId,
    string Code,
    int UsedCount,
    int UsageLimit) : IDomainEvent;
