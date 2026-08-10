
namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Rich Value Object representing a physical address.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string Ward { get; }
    public string District { get; }
    public string City { get; }
    public string Province { get; }
    public string Country { get; }
    public string? PostalCode { get; }

    private Address(
        string street,
        string ward,
        string district,
        string city,
        string province,
        string country,
        string? postalCode)
    {
        Street = street;
        Ward = ward;
        District = district;
        City = city;
        Province = province;
        Country = country;
        PostalCode = postalCode;
    }

    public static Address Create(
        string street,
        string city,
        string province,
        string country,
        string? postalCode = null,
        string? ward = null,
        string? district = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException(
                "Street is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(
                "City is required.");

        if (string.IsNullOrWhiteSpace(province))
            throw new DomainException(
                "Province is required.");

        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException(
                "Country is required.");

        var normalizedCountry = country
            .Trim()
            .ToUpperInvariant();

        if (normalizedCountry.Length != 2)
            throw new DomainException(
                "Country must be a valid ISO 3166-1 alpha-2 code.");

        return new Address(
            street: NormalizeRequired(street),
            ward: NormalizeOptional(ward),
            district: NormalizeOptional(district),
            city: NormalizeRequired(city),
            province: NormalizeRequired(province),
            country: normalizedCountry,
            postalCode: NormalizeOptional(postalCode));
    }

    public bool IsDomestic(string countryCode = "VN")
    {
        return Country.Equals(
            countryCode.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    public string ToFormattedString()
    {
        var parts = new List<string>
        {
            Street
        };

        AddIfNotEmpty(parts, Ward);
        AddIfNotEmpty(parts, District);

        parts.Add(City);
        parts.Add(Province);

        AddIfNotEmpty(parts, PostalCode);

        parts.Add(Country);

        return string.Join(", ", parts);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Ward;
        yield return District;
        yield return City;
        yield return Province;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString()
        => ToFormattedString();

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string NormalizeOptional(string? value)
        => value?.Trim() ?? string.Empty;

    private static void AddIfNotEmpty(
        ICollection<string> parts,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            parts.Add(value);
    }
}
