using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Categories;

public sealed record CategoryCreatedEvent(
    Guid CategoryId,
    string Name,
    Guid? ParentCategoryId) : DomainEvent;
