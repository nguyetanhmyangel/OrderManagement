using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.ReturnOrders;

public static class ReturnOrderErrors
{
    public static Error NotFound(Guid returnOrderId) => Error.NotFound(
        "ReturnOrders.NotFound",
        $"The return order with Id = '{returnOrderId}' was not found.");

    public static Error EmptyItems => Error.Validation(
        "ReturnOrders.EmptyItems",
        "Return order phải có ít nhất một sản phẩm.");

    public static Error ItemAlreadyAdded => Error.Conflict(
        "ReturnOrders.ItemAlreadyAdded",
        "Sản phẩm này đã được thêm vào yêu cầu trả hàng.");

    public static Error InvalidQuantity => Error.Validation(
        "ReturnOrders.InvalidQuantity",
        "Số lượng trả phải lớn hơn 0.");

    public static Error NotModifiable(ReturnStatus status) => Error.Problem(
        "ReturnOrders.NotModifiable",
        $"Không thể chỉnh sửa return order ở trạng thái {status}.");

    public static Error CannotApprove(ReturnStatus status) => Error.Problem(
        "ReturnOrders.CannotApprove",
        $"Chỉ return Requested mới được approve. Hiện tại: {status}.");

    public static Error CannotReject(ReturnStatus status) => Error.Problem(
        "ReturnOrders.CannotReject",
        $"Chỉ return Requested mới được reject. Hiện tại: {status}.");

    public static Error RejectReasonRequired => Error.Validation(
        "ReturnOrders.RejectReasonRequired",
        "Lý do từ chối không được để trống.");

    public static Error CannotMarkReceived(ReturnStatus status) => Error.Problem(
        "ReturnOrders.CannotMarkReceived",
        $"Chỉ return Approved mới được đánh dấu received. Hiện tại: {status}.");

    public static Error CannotMarkRefunded(ReturnStatus status) => Error.Problem(
        "ReturnOrders.CannotMarkRefunded",
        $"Chỉ return Received mới được đánh dấu refunded. Hiện tại: {status}.");

    public static Error CannotClose(ReturnStatus status) => Error.Problem(
        "ReturnOrders.CannotClose",
        $"Chỉ return Refunded hoặc Rejected mới được đóng. Hiện tại: {status}.");
}
