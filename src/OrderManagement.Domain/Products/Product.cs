using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Products;

/// <summary>
/// Aggregate Root — Product.
/// Owns: ProductImage.
/// References (Id only): Category, Brand.
/// </summary>
public sealed class Product : Entity<Guid>, IAggregateRoot
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ShortDescription { get; private set; }
    public Money Price { get; private set; } = null!;
    public Money? CompareAtPrice { get; private set; }
    public decimal WeightKg { get; private set; }

    // Cross-AR references — Id only
    public Guid? CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ProductImage> _images = [];
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    private Product() { }

    public static Product Create(
        string sku,
        string name,
        Money price,
        decimal weightKg,
        string? description = null,
        string? shortDescription = null,
        Money? compareAtPrice = null,
        Guid? categoryId = null,
        Guid? brandId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException(ProductErrors.SkuRequired.Description);
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.NameRequired.Description);
        if (price.Amount <= 0)
            throw new DomainException(ProductErrors.InvalidPrice.Description);
        if (weightKg <= 0)
            throw new DomainException(ProductErrors.InvalidWeight.Description);
        if (compareAtPrice is not null && compareAtPrice.Amount < price.Amount)
            throw new DomainException(ProductErrors.InvalidCompareAtPrice.Description);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ShortDescription = shortDescription?.Trim(),
            Price = price,
            CompareAtPrice = compareAtPrice,
            WeightKg = weightKg,
            CategoryId = categoryId,
            BrandId = brandId,
            IsActive = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(
            product.Id, product.Sku, product.Name,
            product.Price.Amount, product.Price.Currency));

        return product;
    }

    public void UpdateInfo(string name, string? description, string? shortDescription, decimal weightKg)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.NameRequired.Description);
        if (weightKg <= 0)
            throw new DomainException(ProductErrors.InvalidWeight.Description);

        Name = name.Trim();
        Description = description?.Trim();
        ShortDescription = shortDescription?.Trim();
        WeightKg = weightKg;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice, Money? compareAtPrice = null)
    {
        if (newPrice.Amount <= 0)
            throw new DomainException(ProductErrors.InvalidPrice.Description);
        if (compareAtPrice is not null && compareAtPrice.Amount < newPrice.Amount)
            throw new DomainException(ProductErrors.InvalidCompareAtPrice.Description);

        var old = Price.Amount;
        Price = newPrice;
        CompareAtPrice = compareAtPrice;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductPriceChangedEvent(Id, old, newPrice.Amount, newPrice.Currency));
    }

    public void AssignCategory(Guid? categoryId) { CategoryId = categoryId; UpdatedAt = DateTime.UtcNow; }
    public void AssignBrand(Guid? brandId) { BrandId = brandId; UpdatedAt = DateTime.UtcNow; }

    public ProductImage AddImage(string url, string? altText = null, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException(ProductErrors.ImageUrlRequired.Description);

        if (isPrimary)
            foreach (var img in _images) img.UnsetPrimary();

        var image = ProductImage.Create(Id, url, altText, _images.Count, isPrimary);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductImageAddedEvent(Id, image.Id, image.Url, image.IsPrimary));
        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException(ProductErrors.ImageNotFound(imageId).Description);
        _images.Remove(image);
        if (image.IsPrimary && _images.Count > 0) _images[0].SetAsPrimary();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException(ProductErrors.ImageNotFound(imageId).Description);
        foreach (var img in _images) img.UnsetPrimary();
        image.SetAsPrimary();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Feature() { IsFeatured = true; UpdatedAt = DateTime.UtcNow; }
    public void Unfeature() { IsFeatured = false; UpdatedAt = DateTime.UtcNow; }

    public void Activate()
    {
        if (IsActive) throw new DomainException(ProductErrors.AlreadyActive(Id).Description);
        IsActive = true; UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) throw new DomainException(ProductErrors.AlreadyInactive(Id).Description);
        IsActive = false; UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductDeactivatedEvent(Id));
    }

    public decimal? DiscountPercentage =>
        CompareAtPrice is null || CompareAtPrice.Amount <= Price.Amount
            ? null
            : Math.Round((CompareAtPrice.Amount - Price.Amount) / CompareAtPrice.Amount * 100, 0);
}
