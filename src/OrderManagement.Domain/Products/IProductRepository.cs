
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Domain.Products;

public interface IProductRepository: IRepository<Product, Guid>
{

}
