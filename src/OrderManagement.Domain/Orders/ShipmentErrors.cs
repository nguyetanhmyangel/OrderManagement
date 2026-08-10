using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Orders;

public static class ShipmentErrors
{
    public static Error NotFound(Guid shipmentId) => Error.NotFound(
        "Shipments.NotFound",
        $"The shipment with Id = '{shipmentId}' was not found.");

    public static Error AddressRequired => Error.Validation(
        "Shipments.AddressRequired",
        "Địa chỉ giao hàng không được null.");

    public static Error ZoneRequired => Error.Validation(
        "Shipments.ZoneRequired",
        "Shipping zone không được null.");

    public static Error TrackingNumberRequired => Error.Validation(
        "Shipments.TrackingNumberRequired",
        "Mã tracking không được để trống.");

    public static Error CarrierRequired => Error.Validation(
        "Shipments.CarrierRequired",
        "Đơn vị vận chuyển không được để trống.");

    public static Error InvalidTransition(ShipmentStatus from, ShipmentStatus to) => Error.Problem(
        "Shipments.InvalidTransition",
        $"Không thể chuyển shipment từ {from} sang {to}.");

    public static Error AlreadyDelivered => Error.Problem(
        "Shipments.AlreadyDelivered",
        "Không thể đánh dấu failed shipment đã delivered.");
}
