using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Orders;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) => Error.NotFound(
        "Payments.NotFound",
        $"The payment with Id = '{paymentId}' was not found.");

    public static Error InvalidAmount => Error.Validation(
        "Payments.InvalidAmount",
        "Số tiền thanh toán phải lớn hơn 0.");

    public static Error CannotAuthorize(PaymentStatus status) => Error.Problem(
        "Payments.CannotAuthorize",
        $"Chỉ payment Pending mới có thể authorize. Hiện tại: {status}.");

    public static Error CannotCapture(PaymentStatus status) => Error.Problem(
        "Payments.CannotCapture",
        $"Không thể capture payment ở trạng thái {status}.");

    public static Error CannotFail(PaymentStatus status) => Error.Problem(
        "Payments.CannotFail",
        $"Không thể đánh dấu failed payment đã {status}.");

    public static Error CannotCancel(PaymentStatus status) => Error.Problem(
        "Payments.CannotCancel",
        $"Không thể hủy payment đã {status}.");

    public static Error TransactionIdRequired => Error.Validation(
        "Payments.TransactionIdRequired",
        "TransactionId không được để trống.");

    public static Error CannotRefund(PaymentStatus status) => Error.Problem(
        "Payments.CannotRefund",
        "Chỉ payment đã capture mới được hoàn tiền.");

    public static Error RefundExceedsRemaining(decimal remaining, decimal requested) => Error.Problem(
        "Payments.RefundExceedsRemaining",
        $"Số tiền hoàn vượt quá số còn lại. Còn lại: {remaining}, yêu cầu: {requested}.");
}
