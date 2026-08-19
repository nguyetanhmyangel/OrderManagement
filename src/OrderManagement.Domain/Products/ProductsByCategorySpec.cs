using OrderManagement.Domain.Specifications;

namespace OrderManagement.Domain.Products;

public class ProductsByCategorySpec : BaseSpecification<Product>
{
    public ProductsByCategorySpec(Guid categoryId, int page, int pageSize)
        : base(p => p.CategoryId == categoryId && p.IsActive)
    {
        ApplyOrderBy(p => p.Name);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
