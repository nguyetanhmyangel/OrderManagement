using OrderManagement.Domain.Specifications;

namespace OrderManagement.Domain.Products;

public class ProductByIdSpec : BaseSpecification<Product>
{
    public ProductByIdSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Images);
    }
}
