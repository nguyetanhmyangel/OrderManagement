
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Vouchers;

public static class VoucherErrors
{
    public static Error NotFound(Guid voucherId) => Error.NotFound(
        "Vouchers.NotFound",
        $"The voucher with Id = '{voucherId}' was not found.");

    public static Error NotFoundByCode(string code) => Error.NotFound(
        "Vouchers.NotFoundByCode",
        $"The voucher with code = '{code}' was not found.");

    public static Error CodeRequired => Error.Validation(
        "Vouchers.CodeRequired",
        "Mã voucher không được để trống.");

    public static Error InvalidDiscountValue => Error.Validation(
        "Vouchers.InvalidDiscountValue",
        "Giá trị giảm giá phải lớn hơn 0.");

    public static Error PercentageExceeds100 => Error.Validation(
        "Vouchers.PercentageExceeds100",
        "Phần trăm giảm giá không thể vượt quá 100%.");

    public static Error InvalidDateRange => Error.Validation(
        "Vouchers.InvalidDateRange",
        "Ngày bắt đầu phải trước ngày kết thúc.");

    public static Error InvalidUsageLimit => Error.Validation(
        "Vouchers.InvalidUsageLimit",
        "Giới hạn sử dụng phải lớn hơn 0.");

    public static Error CodeAlreadyExists(string code) => Error.Conflict(
        "Vouchers.CodeAlreadyExists",
        $"Voucher code '{code}' already exists.");

    public static Error Inactive => Error.Problem(
        "Vouchers.Inactive",
        "Voucher không còn hoạt động.");

    public static Error Expired(DateTime validFrom, DateTime validTo) => Error.Problem(
        "Vouchers.Expired",
        $"Voucher chỉ có hiệu lực từ {validFrom:dd/MM/yyyy} đến {validTo:dd/MM/yyyy}.");

    public static Error UsageLimitReached => Error.Problem(
        "Vouchers.UsageLimitReached",
        "Voucher đã hết lượt sử dụng.");

    public static Error MinimumOrderNotMet(string minimum) => Error.Problem(
        "Vouchers.MinimumOrderNotMet",
        $"Giá trị đơn hàng tối thiểu phải là {minimum}.");
}
