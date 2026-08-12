using OrderManagement.Domain.orders;

namespace OrderManagement.Domain.Repositories;

public interface IOrderRepository: IRepository<Order, Guid>
{

}
