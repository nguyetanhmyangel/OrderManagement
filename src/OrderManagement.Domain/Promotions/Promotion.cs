
using OrderManagement.Domain.Customers;
using OrderManagement.Domain.Vouchers;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Promotions;

/// <summary>
/// Aggregate Root — Promotion campaign.
/// Can generate Voucher codes (Voucher remains its own AR; only PromotionId link).
/// </summary>
public sealed class Promotion : Entity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PromotionType Type { get; private set; }
    public decimal DiscountValue { get; private set; }
    public Money? MinimumOrderValue { get; private set; }
    public Money? MaximumDiscountAmount { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Cross-AR applicability filters — Id only
    public Guid? ApplicableCategoryId { get; private set; }
    public Guid? ApplicableBrandId { get; private set; }
    public Guid? ApplicableProductId { get; private set; }

    private Promotion() { }

    public static Promotion Create(
        string name, PromotionType type, decimal discountValue,
        DateTime validFrom, DateTime validTo,
        string? description = null,
        Money? minimumOrderValue = null, Money? maximumDiscountAmount = null,
        int? usageLimit = null,
        Guid? applicableCategoryId = null, Guid? applicableBrandId = null,
        Guid? applicableProductId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(PromotionErrors.NameRequired.Description);
        if (discountValue <= 0)
            throw new DomainException(PromotionErrors.InvalidDiscountValue.Description);
        if (type == PromotionType.PercentageDiscount && discountValue > 100)
            throw new DomainException(PromotionErrors.PercentageExceeds100.Description);
        if (validFrom >= validTo)
            throw new DomainException(PromotionErrors.InvalidDateRange.Description);

        var p = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            DiscountValue = discountValue,
            MinimumOrderValue = minimumOrderValue,
            MaximumDiscountAmount = maximumDiscountAmount,
            ValidFrom = validFrom,
            ValidTo = validTo,
            UsageLimit = usageLimit,
            UsedCount = 0,
            IsActive = true,
            ApplicableCategoryId = applicableCategoryId,
            ApplicableBrandId = applicableBrandId,
            ApplicableProductId = applicableProductId,
            CreatedAt = DateTime.UtcNow
        };
        p.RaiseDomainEvent(new PromotionCreatedEvent(p.Id, p.Name, p.ValidFrom, p.ValidTo));
        return p;
    }

    /// <summary>Factory helper — returns a new Voucher AR linked by PromotionId.</summary>
    public Voucher GenerateVoucher(string code, int usageLimit, int? usageLimitPerCustomer = null)
    {
        var voucherType = Type switch
        {
            PromotionType.PercentageDiscount => VoucherType.Percentage,
            PromotionType.FixedAmountDiscount => VoucherType.FixedAmount,
            PromotionType.FreeShipping => VoucherType.FreeShipping,
            _ => VoucherType.Percentage
        };

        return Voucher.Create(
            code, voucherType, DiscountValue, ValidFrom, ValidTo, usageLimit,
            MinimumOrderValue, MaximumDiscountAmount, usageLimitPerCustomer, Id);
    }

    public Money CalculateDiscount(Money orderAmount)
    {
        EnsureValid(orderAmount);
        Money discount = Type switch
        {
            PromotionType.PercentageDiscount => orderAmount.Percentage(DiscountValue),
            PromotionType.FixedAmountDiscount => Money.Create(DiscountValue, orderAmount.Currency),
            PromotionType.FreeShipping => Money.ZeroOf(orderAmount.Currency),
            _ => throw new DomainException($"Loại promotion {Type} cần logic riêng.")
        };
        if (MaximumDiscountAmount is not null && discount.IsGreaterThan(MaximumDiscountAmount))
            discount = MaximumDiscountAmount;
        if (discount.IsGreaterThan(orderAmount))
            discount = orderAmount;
        return discount;
    }

    public void MarkAsUsed()
    {
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
            throw new DomainException(PromotionErrors.UsageLimitReached.Description);
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public bool IsCurrentlyValid =>
        IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo
        && (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);

    private void EnsureValid(Money orderAmount)
    {
        if (!IsActive)
            throw new DomainException(PromotionErrors.Inactive.Description);
        var now = DateTime.UtcNow;
        if (now < ValidFrom || now > ValidTo)
            throw new DomainException(PromotionErrors.Expired(ValidFrom, ValidTo).Description);
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
            throw new DomainException(PromotionErrors.UsageLimitReached.Description);
        if (MinimumOrderValue is not null && orderAmount.IsLessThan(MinimumOrderValue))
            throw new DomainException(PromotionErrors.MinimumOrderNotMet(MinimumOrderValue.ToString()).Description);
    }
}
