
using OrderManagement.Domain.Orders;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Orders.NotFound",
        $"The order with Id = '{orderId}' was not found.");

    public static Error CustomerNotFound(Guid customerId) => Error.NotFound(
        "Orders.CustomerNotFound",
        $"The customer with Id = '{customerId}' was not found.");

    public static Error ShippingAddressRequired => Error.Validation(
        "Orders.ShippingAddressRequired",
        "Địa chỉ giao hàng không được null.");

    public static Error EmptyItems => Error.Validation(
        "Orders.EmptyItems",
        "Đơn hàng phải có ít nhất một sản phẩm.");

    public static Error ItemNotFound(Guid orderItemId) => Error.NotFound(
        "Orders.ItemNotFound",
        $"Không tìm thấy item '{orderItemId}'.");

    public static Error InvalidQuantity => Error.Validation(
        "Orders.InvalidQuantity",
        "Số lượng phải lớn hơn 0.");

    public static Error NotModifiable(OrderStatus status) => Error.Problem(
        "Orders.NotModifiable",
        $"Không thể chỉnh sửa Order ở trạng thái {status}. Chỉ Draft mới được sửa.");

    public static Error CannotPlace(OrderStatus status) => Error.Problem(
        "Orders.CannotPlace",
        $"Chỉ có thể Place đơn ở trạng thái Draft. Hiện tại: {status}.");

    public static Error CannotConfirm(OrderStatus status) => Error.Problem(
        "Orders.CannotConfirm",
        $"Chỉ có thể Confirm đơn đã Placed. Hiện tại: {status}.");

    public static Error CannotMarkAsPaid(OrderStatus status) => Error.Problem(
        "Orders.CannotMarkAsPaid",
        $"Không thể đánh dấu paid ở trạng thái {status}.");

    public static Error CannotShip(OrderStatus status) => Error.Problem(
        "Orders.CannotShip",
        $"Chỉ đơn Processing/Paid/Confirmed mới được ship. Hiện tại: {status}.");

    public static Error AlreadyHasShipment => Error.Problem(
        "Orders.AlreadyHasShipment",
        "Đơn hàng đã có thông tin shipment.");

    public static Error CannotDeliver(OrderStatus status) => Error.Problem(
        "Orders.CannotDeliver",
        $"Chỉ đơn Shipped mới được đánh dấu Delivered. Hiện tại: {status}.");

    public static Error CannotComplete(OrderStatus status) => Error.Problem(
        "Orders.CannotComplete",
        $"Chỉ đơn Delivered mới được Complete. Hiện tại: {status}.");

    public static Error CannotCancel(OrderStatus status) => Error.Problem(
        "Orders.CannotCancel",
        $"Không thể hủy đơn ở trạng thái {status}.");

    public static Error CancellationReasonRequired => Error.Validation(
        "Orders.CancellationReasonRequired",
        "Lý do hủy không được để trống.");

    public static Error CannotReturn(OrderStatus status) => Error.Problem(
        "Orders.CannotReturn",
        $"Chỉ đơn Delivered/Completed mới có thể returned. Hiện tại: {status}.");

    public static Error VoucherRequired => Error.Validation(
        "Orders.VoucherRequired",
        "Voucher không được null.");
}
