namespace OrderManagement.Domain.Orders;

public enum ShipmentStatus
{
    Pending = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5,
    ReturnedToSender = 6
}
