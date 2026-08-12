using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Categories;

public sealed record CategoryCreatedEvent(
    Guid CategoryId,
    string Name,
    Guid? ParentCategoryId,
    Guid EventId,
    DateTime OccurredOnUtc) : IDomainEvent
{
    // Constructor tiện lợi cho Domain: Tự gán EventId & UtcNow nếu không truyền vào
    public CategoryCreatedEvent(Guid categoryId, string name, Guid? parentCategoryId)
        : this(categoryId, name, parentCategoryId, Guid.NewGuid(), DateTime.UtcNow) { }
}

