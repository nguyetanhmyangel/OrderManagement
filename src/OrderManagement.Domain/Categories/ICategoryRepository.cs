using OrderManagement.Domain.Repositories;

namespace OrderManagement.Domain.Categories;

public interface ICategoryRepository: IRepository<Category, Guid>
{

}
