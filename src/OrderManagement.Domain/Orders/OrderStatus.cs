namespace OrderManagement.Domain.Orders;

public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Confirmed = 2,
    Paid = 3,
    Processing = 4,
    Shipped = 5,
    Delivered = 6,
    Completed = 7,
    Cancelled = 8,
    Returned = 9
}
