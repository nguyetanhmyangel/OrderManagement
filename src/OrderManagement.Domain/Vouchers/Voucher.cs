
using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Vouchers;

/// <summary>
/// Aggregate Root (standalone) or child of Promotion.
/// References Promotion by Id only when linked.
/// </summary>
public sealed class Voucher : Entity, IAggregateRoot
{
    public string Code { get; private set; } = null!;
    public VoucherType Type { get; private set; }
    public decimal DiscountValue { get; private set; }
    public Money? MinimumOrderValue { get; private set; }
    public Money? MaximumDiscountAmount { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public int UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public int? UsageLimitPerCustomer { get; private set; }
    public Guid? PromotionId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Voucher() { }

    public static Voucher Create(
        string code, VoucherType type, decimal discountValue,
        DateTime validFrom, DateTime validTo, int usageLimit,
        Money? minimumOrderValue = null, Money? maximumDiscountAmount = null,
        int? usageLimitPerCustomer = null, Guid? promotionId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(VoucherErrors.CodeRequired.Description);
        if (discountValue <= 0)
            throw new DomainException(VoucherErrors.InvalidDiscountValue.Description);
        if (type == VoucherType.Percentage && discountValue > 100)
            throw new DomainException(VoucherErrors.PercentageExceeds100.Description);
        if (validFrom >= validTo)
            throw new DomainException(VoucherErrors.InvalidDateRange.Description);
        if (usageLimit <= 0)
            throw new DomainException(VoucherErrors.InvalidUsageLimit.Description);

        var v = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Type = type,
            DiscountValue = discountValue,
            MinimumOrderValue = minimumOrderValue,
            MaximumDiscountAmount = maximumDiscountAmount,
            ValidFrom = validFrom,
            ValidTo = validTo,
            UsageLimit = usageLimit,
            UsedCount = 0,
            UsageLimitPerCustomer = usageLimitPerCustomer,
            PromotionId = promotionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        v.RaiseDomainEvent(new VoucherCreatedEvent(v.Id, promotionId, v.Code));
        return v;
    }

    public Money CalculateDiscount(Money orderAmount)
    {
        ValidateCanBeUsed(orderAmount);

        Money discount = Type switch
        {
            VoucherType.Percentage => orderAmount.Percentage(DiscountValue),
            VoucherType.FixedAmount => Money.Create(DiscountValue, orderAmount.Currency),
            VoucherType.FreeShipping => Money.ZeroOf(orderAmount.Currency),
            _ => throw new DomainException($"Loại voucher {Type} không hỗ trợ.")
        };

        if (MaximumDiscountAmount is not null && discount.IsGreaterThan(MaximumDiscountAmount))
            discount = MaximumDiscountAmount;
        if (discount.IsGreaterThan(orderAmount))
            discount = orderAmount;
        return discount;
    }

    public Money Apply(Money orderAmount)
    {
        var discount = CalculateDiscount(orderAmount);
        MarkAsUsed();
        return discount;
    }

    public void MarkAsUsed()
    {
        if (UsedCount >= UsageLimit)
            throw new DomainException(VoucherErrors.UsageLimitReached.Description);
        UsedCount++;
        RaiseDomainEvent(new VoucherUsedEvent(Id, Code, UsedCount, UsageLimit));
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public bool IsValid =>
        IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo && UsedCount < UsageLimit;

    private void ValidateCanBeUsed(Money orderAmount)
    {
        if (!IsActive)
            throw new DomainException(VoucherErrors.Inactive.Description);
        var now = DateTime.UtcNow;
        if (now < ValidFrom || now > ValidTo)
            throw new DomainException(VoucherErrors.Expired(ValidFrom, ValidTo).Description);
        if (UsedCount >= UsageLimit)
            throw new DomainException(VoucherErrors.UsageLimitReached.Description);
        if (MinimumOrderValue is not null && orderAmount.IsLessThan(MinimumOrderValue))
            throw new DomainException(VoucherErrors.MinimumOrderNotMet(MinimumOrderValue.ToString()).Description);
    }
}
