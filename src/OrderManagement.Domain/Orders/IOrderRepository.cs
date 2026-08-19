
using OrderManagement.Domain.orders;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Domain.Orders;

public interface IOrderRepository: IRepository<Order, Guid>
{

}
