

using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Customers;

/// <summary>
/// Rich Value Object representing monetary value.
/// Immutable, currency-aware, and self-validating.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Số tiền không thể âm.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency code phải là 3 ký tự theo chuẩn ISO 4217.");

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero => new(0, "VND");
    public static Money ZeroOf(string currency) => Create(0, currency);
    public static Money FromVND(decimal amount) => Create(amount, "VND");
    public static Money FromUSD(decimal amount) => Create(amount, "USD");

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return this with { Amount = Amount + other.Amount };
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException("Kết quả phép trừ không thể âm.");
        return this with { Amount = result };
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Hệ số nhân không thể âm.");
        return this with { Amount = Math.Round(Amount * factor, 2) };
    }

    public Money Percentage(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new DomainException("Phần trăm phải nằm trong khoảng 0–100.");
        return Multiply(percent / 100m);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount < other.Amount;
    }

    public bool IsZero => Amount == 0;

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Không thể thực hiện phép toán giữa {Currency} và {other.Currency}.");
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
