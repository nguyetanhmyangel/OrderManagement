using OrderManagement.Domain.Categories;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Categories;

/// <summary>Aggregate Root — Category (hierarchy via ParentCategoryId).</summary>
public sealed class Category : Entity<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Category() { }

    private Category(Guid id, string name, string slug, string? description,
        Guid? parentCategoryId, int displayOrder, bool isActive,
        DateTime createAt, DateTime? updateAt) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAt = createAt;
        UpdatedAt = updateAt;
    }

    public static Category Create(
        string name, string? description = null,
        Guid? parentCategoryId = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CategoryErrors.NameRequired.Description);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = GenerateSlug(name),
            Description = description?.Trim(),
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        category.RaiseDomainEvent(new CategoryCreatedEvent(category.Id, category.Name, category.ParentCategoryId));
        return category;
    }

    public void Update(string name, string? description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CategoryErrors.NameRequired.Description);
        Name = name.Trim();
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToParent(Guid? newParentId)
    {
        if (newParentId == Id)
            throw new DomainException(CategoryErrors.CannotBeOwnParent.Description);
        ParentCategoryId = newParentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    private static string GenerateSlug(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("--", "-");
}
