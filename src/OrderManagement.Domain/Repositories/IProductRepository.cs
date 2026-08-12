using OrderManagement.Domain.Products;

namespace OrderManagement.Domain.Repositories;

public interface IProductRepository: IRepository<Product, Guid>
{

}
