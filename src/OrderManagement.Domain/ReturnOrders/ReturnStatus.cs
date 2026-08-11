namespace OrderManagement.Domain.ReturnOrders;

public enum ReturnStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Received = 3,
    Refunded = 4,
    Closed = 5
}
