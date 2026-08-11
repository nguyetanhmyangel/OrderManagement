
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Products;

/// <summary>Entity owned by Product Aggregate.</summary>
public sealed class ProductImage : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProductImage() { }

    private ProductImage(Guid id, Guid productId, string url, string? altText,
        int displayOrder, bool isPrimary) : base(id)
    {
        ProductId = productId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        CreatedAt = DateTime.UtcNow;
    }

    internal static ProductImage Create(
        Guid productId, string url, string? altText = null,
        int displayOrder = 0, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("URL hình ảnh không được để trống.");

        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url.Trim(),
            AltText = altText?.Trim(),
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void SetAsPrimary() => IsPrimary = true;
    internal void UnsetPrimary() => IsPrimary = false;

    public void Update(string url, string? altText, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("URL hình ảnh không được để trống.");
        Url = url.Trim();
        AltText = altText?.Trim();
        DisplayOrder = displayOrder;
    }
}
