
using System.Text.RegularExpressions;

namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Value Object for validated phone numbers (Vietnam-focused).
/// </summary>
public sealed record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Số điện thoại không được để trống.");

        var cleaned = Regex.Replace(phone, @"[\s\-\.\(\)]", "");

        // Accept +84... or 0...
        if (!Regex.IsMatch(cleaned, @"^(\+84|0)[3|5|7|8|9]\d{8}$"))
            throw new DomainException("Số điện thoại không hợp lệ (định dạng Việt Nam).");

        return new PhoneNumber(cleaned);
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
