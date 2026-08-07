
namespace OrderManagement.SharedKernel.ValueObjects;

/// <summary>
/// Value Object for validated email addresses.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email không được để trống.");

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email.Trim())
                throw new DomainException("Email không hợp lệ.");
        }
        catch
        {
            throw new DomainException("Email không hợp lệ.");
        }

        return new Email(email.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
