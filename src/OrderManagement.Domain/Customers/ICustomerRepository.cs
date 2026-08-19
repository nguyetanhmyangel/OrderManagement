
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Domain.Customers;

public interface ICustomerRepository: IRepository<Customer, Guid>
{

}
