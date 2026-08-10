
using System.Text.RegularExpressions;

namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Value Object for validated phone numbers (Vietnam-focused).
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException(
                "Số điện thoại không được để trống.");

        var normalized = Normalize(phone);

        if (!IsValidVietnamesePhone(normalized))
            throw new DomainException(
                "Số điện thoại không hợp lệ.");

        return new PhoneNumber(normalized);
    }

    private static string Normalize(string phone)
    {
        return Regex.Replace(
            phone.Trim(),
            @"[\s\-\.\(\)]",
            string.Empty);
    }

    private static bool IsValidVietnamesePhone(string phone)
    {
        return Regex.IsMatch(
            phone,
            @"^(?:\+84|0)[35789]\d{8}$");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(
        PhoneNumber phone)
        => phone.Value;
}
