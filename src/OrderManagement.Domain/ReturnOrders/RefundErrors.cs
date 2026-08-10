using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.ReturnOrders;

public static class RefundErrors
{
    public static Error NotFound(Guid refundId) => Error.NotFound(
        "Refunds.NotFound",
        $"The refund with Id = '{refundId}' was not found.");

    public static Error InvalidAmount => Error.Validation(
        "Refunds.InvalidAmount",
        "Số tiền hoàn phải lớn hơn 0.");

    public static Error CannotProcess(RefundStatus status) => Error.Problem(
        "Refunds.CannotProcess",
        $"Chỉ refund Pending mới chuyển sang Processing. Hiện tại: {status}.");

    public static Error CannotComplete(RefundStatus status) => Error.Problem(
        "Refunds.CannotComplete",
        $"Không thể complete refund ở trạng thái {status}.");

    public static Error CannotFail(RefundStatus status) => Error.Problem(
        "Refunds.CannotFail",
        "Không thể đánh dấu failed refund đã completed.");
}
