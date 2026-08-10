
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Inventories;

public static class InventoryErrors
{
    public static Error NotFound(Guid inventoryId) => Error.NotFound(
        "Inventories.NotFound",
        $"The inventory with Id = '{inventoryId}' was not found.");

    public static Error NotFoundForProduct(Guid productId, Guid warehouseId) => Error.NotFound(
        "Inventories.NotFoundForProduct",
        $"Inventory for product '{productId}' in warehouse '{warehouseId}' was not found.");

    public static Error InvalidQuantity => Error.Validation(
        "Inventories.InvalidQuantity",
        "Số lượng phải lớn hơn 0.");

    public static Error InsufficientStock(int available, int requested) => Error.Problem(
        "Inventories.InsufficientStock",
        $"Không đủ hàng trong kho. Hiện có: {available}, cần: {requested}.");

    public static Error InsufficientAvailable(int available, int requested) => Error.Problem(
        "Inventories.InsufficientAvailable",
        $"Không đủ hàng khả dụng. Available: {available}, cần: {requested}.");

    public static Error InsufficientReserved(int reserved, int requested) => Error.Problem(
        "Inventories.InsufficientReserved",
        $"Không thể release/confirm nhiều hơn số đã reserve. Reserved: {reserved}, yêu cầu: {requested}.");

    public static Error AdjustBelowReserved(int reserved) => Error.Problem(
        "Inventories.AdjustBelowReserved",
        $"Không thể điều chỉnh thấp hơn số đã reserve ({reserved}).");

    public static Error InvalidReorderSettings => Error.Validation(
        "Inventories.InvalidReorderSettings",
        "Cài đặt reorder không hợp lệ.");

    public static Error NegativeQuantity => Error.Validation(
        "Inventories.NegativeQuantity",
        "Số lượng tồn kho không thể âm.");
}
