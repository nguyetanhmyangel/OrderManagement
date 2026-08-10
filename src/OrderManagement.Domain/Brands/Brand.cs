
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Brands;

/// <summary>Aggregate Root — Brand.</summary>
public sealed class Brand :  Entity<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Constructor rỗng dành cho Dapper/ORM mapping
    private Brand() { }

    // Constructor private dùng trong Factory Method Create
    private Brand(Guid id, string name, Slug slug, string? description, string? logoUrl, string? website)
        : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        LogoUrl = logoUrl;
        Website = website;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Brand Create(
        string name, string? description = null,
        string? logoUrl = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(BrandErrors.NameRequired.Description);

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            LogoUrl = logoUrl,
            Website = website,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        brand.RaiseDomainEvent(new BrandCreatedEvent(brand.Id, brand.Name));
        return brand;
    }

    public void Update(string name, string? description, string? logoUrl, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(BrandErrors.NameRequired.Description);
        Name = name.Trim();
        Slug = Slug.FromName(name);
        Description = description?.Trim();
        LogoUrl = logoUrl;
        Website = website;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    private static string GenerateSlug(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("--", "-");
}
