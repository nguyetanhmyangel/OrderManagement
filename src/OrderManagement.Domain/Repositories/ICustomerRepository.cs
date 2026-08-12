using OrderManagement.Domain.Customers;

namespace OrderManagement.Domain.Repositories;

public interface ICustomerRepository: IRepository<Customer, Guid>
{

}
