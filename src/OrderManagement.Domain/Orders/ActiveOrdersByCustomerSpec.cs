using OrderManagement.Domain.orders;
using OrderManagement.Domain.Specifications;

namespace OrderManagement.Domain.Orders;

public sealed class ActiveOrdersByCustomerSpecification : BaseSpecification<Order>
{
    public ActiveOrdersByCustomerSpecification(Guid customerId)
        : base(order => (order.Status == OrderStatus.Placed
                         || order.Status == OrderStatus.Confirmed)
                        && order.CustomerId == customerId)
    {
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}
