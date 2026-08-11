namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Rich Value Object representing monetary value.
/// Immutable, currency-aware, and self-validating.
/// </summary>

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public string Currency { get; }

    private Money(
        decimal amount,
        string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(
        decimal amount,
        string currency)
    {
        if (amount < 0)
        {
            throw new DomainException(
                "Số tiền không thể âm.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException(
                "Currency không được để trống.");
        }

        var normalizedCurrency =
            currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new DomainException(
                "Currency code phải có 3 ký tự " +
                "theo chuẩn ISO 4217.");
        }

        var normalizedAmount =
            Math.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero);

        return new Money(
            normalizedAmount,
            normalizedCurrency);
    }

    public static Money ZeroOf(string currency)
        => Create(0m, currency);

    public static Money FromVND(decimal amount)
        => Create(amount, "VND");

    public static Money FromUSD(decimal amount)
        => Create(amount, "USD");

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Create(
            Amount + other.Amount,
            Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        if (other.Amount > Amount)
        {
            throw new DomainException(
                "Kết quả phép trừ không thể âm.");
        }

        return Create(
            Amount - other.Amount,
            Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new DomainException(
                "Hệ số nhân không thể âm.");
        }

        return Create(
            Amount * factor,
            Currency);
    }

    public Money Percentage(decimal percent)
    {
        if (percent < 0 || percent > 100)
        {
            throw new DomainException(
                "Phần trăm phải nằm trong khoảng 0–100.");
        }

        return Multiply(percent / 100m);
    }

    public bool IsGreaterThan(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Amount > other.Amount;
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Amount >= other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Amount < other.Amount;
    }

    public bool IsLessThanOrEqual(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        EnsureSameCurrency(other);

        return Amount <= other.Amount;
    }

    public bool IsZero =>
        Amount == 0m;

    public bool IsPositive =>
        Amount > 0m;

    public static Money operator +(
        Money left,
        Money right)
        => left.Add(right);

    public static Money operator -(
        Money left,
        Money right)
        => left.Subtract(right);

    public static Money operator *(
        Money money,
        decimal factor)
        => money.Multiply(factor);

    private void EnsureSameCurrency(
        Money other)
    {
        if (!string.Equals(
                Currency,
                other.Currency,
                StringComparison.Ordinal))
        {
            throw new DomainException(
                $"Không thể thực hiện phép toán giữa " +
                $"{Currency} và {other.Currency}.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
        => $"{Amount:N2} {Currency}";
}

