
using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Vouchers;

/// <summary>
/// Aggregate Root — Phiếu giảm giá (standalone hoặc gắn Promotion qua PromotionId).
/// Không navigation tới Promotion entity.
/// </summary>
public sealed class Voucher : Entity<Guid>, IAggregateRoot
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

    /// <summary>Reference tới Promotion AR — Id only.</summary>
    public Guid? PromotionId { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Concurrency token
    public uint Version { get; private set; }   // chỉ cần khai báo, không cần [Timestamp], vì cấu hình Fluent API
    private Voucher() { }

    private Voucher(Guid id,
        string code,
        VoucherType type,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo,
        int usageLimit,
        int usedCount,
        Money? minimumOrderValue,
        Money? maximumDiscountAmount,
        int? usageLimitPerCustomer,
        Guid? promotionId,
        DateTime createAt) : base(id)
    {
        Code = code;
        Type = type;
        DiscountValue = discountValue;
        MinimumOrderValue = minimumOrderValue;
        MaximumDiscountAmount = maximumDiscountAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UsageLimit = usageLimit;
        UsedCount = usedCount;
        UsageLimitPerCustomer = usageLimitPerCustomer;
        PromotionId = promotionId;
        IsActive = true;
        CreatedAt = createAt;
    }

    public static Voucher Create(
        string code,
        VoucherType type,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo,
        int usageLimit,
        Money? minimumOrderValue = null,
        Money? maximumDiscountAmount = null,
        int? usageLimitPerCustomer = null,
        Guid? promotionId = null)
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

        if (usageLimitPerCustomer is <= 0)
            throw new DomainException(VoucherErrors.InvalidUsageLimit.Description);

        if (minimumOrderValue is not null && maximumDiscountAmount is not null
            && !string.Equals(minimumOrderValue.Currency, maximumDiscountAmount.Currency, StringComparison.Ordinal))
            throw new DomainException("MinimumOrderValue và MaximumDiscountAmount phải cùng currency.");

        var voucher = new Voucher
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

        voucher.RaiseDomainEvent(new VoucherCreatedEvent(voucher.Id, promotionId, voucher.Code));
        return voucher;
    }

    /// <summary>Tính discount — không side-effect.</summary>
    public Money CalculateDiscount(Money orderAmount)
    {
        ArgumentNullException.ThrowIfNull(orderAmount);
        ValidateCanBeUsed(orderAmount);

        Money discount = Type switch
        {
            VoucherType.Percentage => orderAmount.Percentage(DiscountValue),
            VoucherType.FixedAmount => Money.Create(DiscountValue, orderAmount.Currency),
            VoucherType.FreeShipping => Money.ZeroOf(orderAmount.Currency),
            _ => throw new DomainException($"Loại voucher {Type} không được hỗ trợ.")
        };

        if (MaximumDiscountAmount is not null)
        {
            if (!string.Equals(MaximumDiscountAmount.Currency, orderAmount.Currency, StringComparison.Ordinal))
                throw new DomainException("MaximumDiscountAmount không cùng currency với đơn hàng.");

            if (discount.IsGreaterThan(MaximumDiscountAmount))
                discount = MaximumDiscountAmount;
        }

        if (discount.IsGreaterThan(orderAmount))
            discount = orderAmount;

        return discount;
    }

    /// <summary>Tính discount + tăng UsedCount.</summary>
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
        IsActive
        && DateTime.UtcNow >= ValidFrom
        && DateTime.UtcNow <= ValidTo
        && UsedCount < UsageLimit;

    private void ValidateCanBeUsed(Money orderAmount)
    {
        if (!IsActive)
            throw new DomainException(VoucherErrors.Inactive.Description);

        var now = DateTime.UtcNow;
        if (now < ValidFrom || now > ValidTo)
            throw new DomainException(VoucherErrors.Expired(ValidFrom, ValidTo).Description);

        if (UsedCount >= UsageLimit)
            throw new DomainException(VoucherErrors.UsageLimitReached.Description);

        if (MinimumOrderValue is not null)
        {
            if (!string.Equals(MinimumOrderValue.Currency, orderAmount.Currency, StringComparison.Ordinal))
                throw new DomainException("MinimumOrderValue không cùng currency với đơn hàng.");

            if (orderAmount.IsLessThan(MinimumOrderValue))
                throw new DomainException(
                    VoucherErrors.MinimumOrderNotMet(MinimumOrderValue.ToString()).Description);
        }
    }
}

