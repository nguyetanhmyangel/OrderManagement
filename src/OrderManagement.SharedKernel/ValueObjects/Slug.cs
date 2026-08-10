using System.Text;

namespace OrderManagement.SharedKernel.ValueObjects;

public sealed class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Slug cannot be empty.");

        var normalized = Normalize(value);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainException("Slug cannot be empty.");

        return new Slug(normalized);
    }

    public static Slug FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty.");

        return new Slug(Normalize(name));
    }

    private static string Normalize(string value)
    {
        var text = value.Trim().ToLowerInvariant();

        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        return builder
            .ToString()
            .Trim('-');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Slug slug)
        => slug.Value;
}
