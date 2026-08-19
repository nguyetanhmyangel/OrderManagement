using OrderManagement.Domain.Specifications;

namespace OrderManagement.Domain.Products;

public class ActiveProductSpec : BaseSpecification<Product>
{
    public ActiveProductSpec() : base(p => p.IsActive)
    {
        ApplyOrderBy(p => p.Name);
    }
}
