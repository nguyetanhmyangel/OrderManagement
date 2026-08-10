using System.Net.Mail;
namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Value Object for validated email addresses.
/// </summary>

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(
                "Email không được để trống.");

        var normalized = email.Trim().ToLowerInvariant();

        if (!IsValid(normalized))
            throw new DomainException(
                "Email không hợp lệ.");

        return new Email(normalized);
    }

    private static bool IsValid(string email)
    {
        try
        {
            var address = new MailAddress(email);

            return string.Equals(
                address.Address,
                email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(Email email)
        => email.Value;
}
