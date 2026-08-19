using OrderManagement.Domain.Vouchers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Promotions;

/// <summary>
/// Aggregate Root — Chương trình khuyến mãi.
/// Không sở hữu Voucher. Chỉ cung cấp factory tạo Voucher AR (snapshot + PromotionId).
/// </summary>
public sealed class Promotion : Entity<Guid> , IAggregateRoot
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
    // Concurrency token
    public uint Version { get; private set; }   // chỉ cần khai báo, không cần [Timestamp], vì cấu hình Fluent API
    // Cross-AR filters — Id only
    public Guid? ApplicableCategoryId { get; private set; }
    public Guid? ApplicableBrandId { get; private set; }
    public Guid? ApplicableProductId { get; private set; }

    private Promotion() { }

    private Promotion(Guid id,
        string name,
        PromotionType type,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo,
        string? description,
        Money? minimumOrderValue,
        Money? maximumDiscountAmount,
        int? usageLimit,
        int usedCount,
        bool isActive,
        Guid? applicableCategoryId,
        Guid? applicableBrandId,
        Guid? applicableProductId) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        DiscountValue = discountValue;
        MinimumOrderValue = minimumOrderValue;
        MaximumDiscountAmount = maximumDiscountAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UsageLimit = usageLimit;
        UsedCount = usedCount;
        IsActive = isActive;
        ApplicableCategoryId = applicableCategoryId;
        ApplicableBrandId = applicableBrandId;
        ApplicableProductId = applicableProductId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Promotion Create(
        string name,
        PromotionType type,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo,
        string? description = null,
        Money? minimumOrderValue = null,
        Money? maximumDiscountAmount = null,
        int? usageLimit = null,
        Guid? applicableCategoryId = null,
        Guid? applicableBrandId = null,
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

        if (usageLimit is <= 0)
            throw new DomainException(PromotionErrors.UsageLimitReached.Description);

        if (minimumOrderValue is not null && maximumDiscountAmount is not null
            && !string.Equals(minimumOrderValue.Currency, maximumDiscountAmount.Currency, StringComparison.Ordinal))
            throw new DomainException("MinimumOrderValue và MaximumDiscountAmount phải cùng currency.");

        var promotion = new Promotion
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

        promotion.RaiseDomainEvent(new PromotionCreatedEvent(
            promotion.Id, promotion.Name, promotion.ValidFrom, promotion.ValidTo));

        return promotion;
    }

    public void Update(
        string name,
        string? description,
        Money? minimumOrderValue,
        Money? maximumDiscountAmount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(PromotionErrors.NameRequired.Description);

        if (minimumOrderValue is not null && maximumDiscountAmount is not null
                                          && !string.Equals(minimumOrderValue.Currency, maximumDiscountAmount.Currency, StringComparison.Ordinal))
            throw new DomainException("MinimumOrderValue và MaximumDiscountAmount phải cùng currency.");

        Name = name.Trim();
        Description = description?.Trim();
        MinimumOrderValue = minimumOrderValue;
        MaximumDiscountAmount = maximumDiscountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory tạo Voucher AR mới — không add vào collection trong Promotion.
    /// Application layer persist Voucher qua IVoucherRepository.
    /// </summary>
    public Voucher GenerateVoucher(
        string code,
        int usageLimit,
        int? usageLimitPerCustomer = null)
    {
        var voucherType = Type switch
        {
            PromotionType.PercentageDiscount => VoucherType.Percentage,
            PromotionType.FixedAmountDiscount => VoucherType.FixedAmount,
            PromotionType.FreeShipping => VoucherType.FreeShipping,
            _ => VoucherType.Percentage
        };

        // Snapshot values + PromotionId only — không ownership
        return Voucher.Create(
            code: code,
            type: voucherType,
            discountValue: DiscountValue,
            validFrom: ValidFrom,
            validTo: ValidTo,
            usageLimit: usageLimit,
            minimumOrderValue: MinimumOrderValue,
            maximumDiscountAmount: MaximumDiscountAmount,
            usageLimitPerCustomer: usageLimitPerCustomer,
            promotionId: Id);
    }

    public Money CalculateDiscount(Money orderAmount)
    {
        ArgumentNullException.ThrowIfNull(orderAmount);
        EnsureIsValid(orderAmount);

        Money discount = Type switch
        {
            PromotionType.PercentageDiscount => orderAmount.Percentage(DiscountValue),
            PromotionType.FixedAmountDiscount => Money.Create(DiscountValue, orderAmount.Currency),
            PromotionType.FreeShipping => Money.ZeroOf(orderAmount.Currency),
            _ => throw new DomainException($"Loại promotion {Type} cần logic riêng.")
        };

        if (MaximumDiscountAmount is not null)
        {
            // Cùng currency đã validate lúc Create; nếu khác currency của order thì báo lỗi
            if (!string.Equals(MaximumDiscountAmount.Currency, orderAmount.Currency, StringComparison.Ordinal))
                throw new DomainException("MaximumDiscountAmount không cùng currency với đơn hàng.");

            if (discount.IsGreaterThan(MaximumDiscountAmount))
                discount = MaximumDiscountAmount;
        }

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

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCurrentlyValid =>
        IsActive
        && DateTime.UtcNow >= ValidFrom
        && DateTime.UtcNow <= ValidTo
        && (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);

    private void EnsureIsValid(Money orderAmount)
    {
        if (!IsActive)
            throw new DomainException(PromotionErrors.Inactive.Description);

        var now = DateTime.UtcNow;
        if (now < ValidFrom || now > ValidTo)
            throw new DomainException(PromotionErrors.Expired(ValidFrom, ValidTo).Description);

        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
            throw new DomainException(PromotionErrors.UsageLimitReached.Description);

        if (MinimumOrderValue is not null)
        {
            if (!string.Equals(MinimumOrderValue.Currency, orderAmount.Currency, StringComparison.Ordinal))
                throw new DomainException("MinimumOrderValue không cùng currency với đơn hàng.");

            if (orderAmount.IsLessThan(MinimumOrderValue))
                throw new DomainException(
                    PromotionErrors.MinimumOrderNotMet(MinimumOrderValue.ToString()).Description);
        }
    }
}


