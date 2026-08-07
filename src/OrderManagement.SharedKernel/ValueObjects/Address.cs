
namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Rich Value Object representing a physical address.
/// </summary>
public sealed record Address
{
    public string Street { get; init; } = string.Empty;
    public string Ward { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? PostalCode { get; init; }

    private Address() { }

    public Address(
        string street,
        string city,
        string province,
        string country,
        string? postalCode = null,
        string ward = "",
        string district = "")
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
        string ward = "",
        string district = "")
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Địa chỉ đường không được để trống.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Thành phố không được để trống.");

        if (string.IsNullOrWhiteSpace(province))
            throw new DomainException("Tỉnh/Thành không được để trống.");

        if (string.IsNullOrWhiteSpace(country) || country.Length != 2)
            throw new DomainException("Country code phải là 2 ký tự (VD: VN, US).");

        return new Address(
            street.Trim(),
            city.Trim(),
            province.Trim(),
            country.ToUpperInvariant().Trim(),
            postalCode?.Trim(),
            ward.Trim(),
            district.Trim());
    }

    public bool IsDomestic() => Country == "VN";

    public string ToFormattedString()
    {
        var parts = new List<string> { Street };

        if (!string.IsNullOrWhiteSpace(Ward))
            parts.Add(Ward);
        if (!string.IsNullOrWhiteSpace(District))
            parts.Add(District);

        parts.Add(City);
        parts.Add(Province);

        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);

        parts.Add(Country);

        return string.Join(", ", parts);
    }

    public override string ToString() => ToFormattedString();
}
