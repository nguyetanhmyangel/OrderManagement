using OrderManagement.Domain.Customers;

namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Value Object representing a shipping zone with rate behavior.
/// </summary>
public sealed record ShippingZone
{
    public string Code { get; }
    public string Name { get; }
    public decimal RatePerKg { get; }

    private ShippingZone(string code, string name, decimal ratePerKg)
    {
        Code = code;
        Name = name;
        RatePerKg = ratePerKg;
    }

    public static readonly ShippingZone Domestic = new("DOMESTIC", "Nội thành", 15_000m);
    public static readonly ShippingZone Regional = new("REGIONAL", "Liên tỉnh", 25_000m);
    public static readonly ShippingZone International = new("INTL", "Quốc tế", 80_000m);

    public Money CalculateShippingFee(decimal weightKg, string currency = "VND")
    {
        if (weightKg <= 0)
            throw new DomainException("Trọng lượng phải lớn hơn 0.");

        var fee = Math.Ceiling(weightKg) * RatePerKg;
        return Money.Create(fee, currency);
    }

    public static ShippingZone FromCode(string code) => code.ToUpperInvariant() switch
    {
        "DOMESTIC" => Domestic,
        "REGIONAL" => Regional,
        "INTL" => International,
        _ => throw new DomainException($"Shipping zone '{code}' không được hỗ trợ.")
    };
}
