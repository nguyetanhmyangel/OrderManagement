
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Promotions;

public static class PromotionErrors
{
    public static Error NotFound(Guid promotionId) => Error.NotFound(
        "Promotions.NotFound",
        $"The promotion with Id = '{promotionId}' was not found.");

    public static Error NameRequired => Error.Validation(
        "Promotions.NameRequired",
        "Tên chương trình khuyến mãi không được để trống.");

    public static Error InvalidDiscountValue => Error.Validation(
        "Promotions.InvalidDiscountValue",
        "Giá trị giảm giá phải lớn hơn 0.");

    public static Error PercentageExceeds100 => Error.Validation(
        "Promotions.PercentageExceeds100",
        "Phần trăm giảm giá không thể vượt quá 100%.");

    public static Error InvalidDateRange => Error.Validation(
        "Promotions.InvalidDateRange",
        "Ngày bắt đầu phải trước ngày kết thúc.");

    public static Error Inactive => Error.Problem(
        "Promotions.Inactive",
        "Promotion không còn hoạt động.");

    public static Error Expired(DateTime validFrom, DateTime validTo) => Error.Problem(
        "Promotions.Expired",
        $"Promotion chỉ có hiệu lực từ {validFrom:dd/MM/yyyy} đến {validTo:dd/MM/yyyy}.");

    public static Error UsageLimitReached => Error.Problem(
        "Promotions.UsageLimitReached",
        "Promotion đã hết lượt sử dụng.");

    public static Error MinimumOrderNotMet(string minimum) => Error.Problem(
        "Promotions.MinimumOrderNotMet",
        $"Giá trị đơn hàng tối thiểu phải là {minimum}.");
}
