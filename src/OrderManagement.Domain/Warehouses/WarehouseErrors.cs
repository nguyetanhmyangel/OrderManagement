
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Warehouses;

public static class WarehouseErrors
{
    public static Error NotFound(Guid warehouseId) => Error.NotFound(
        "Warehouses.NotFound",
        $"The warehouse with Id = '{warehouseId}' was not found.");

    public static Error CodeRequired => Error.Validation(
        "Warehouses.CodeRequired",
        "Mã kho không được để trống.");

    public static Error NameRequired => Error.Validation(
        "Warehouses.NameRequired",
        "Tên kho không được để trống.");

    public static Error AddressRequired => Error.Validation(
        "Warehouses.AddressRequired",
        "Địa chỉ kho không được null.");

    public static Error CodeAlreadyExists(string code) => Error.Conflict(
        "Warehouses.CodeAlreadyExists",
        $"Warehouse code '{code}' already exists.");

    public static Error CannotDeactivateDefault => Error.Problem(
        "Warehouses.CannotDeactivateDefault",
        "Không thể deactivate kho mặc định.");

    public static Error AlreadyActive(Guid warehouseId) => Error.Problem(
        "Warehouses.AlreadyActive",
        $"The warehouse with Id = '{warehouseId}' is already active.");

    public static Error AlreadyInactive(Guid warehouseId) => Error.Problem(
        "Warehouses.AlreadyInactive",
        $"The warehouse with Id = '{warehouseId}' is already inactive.");
}
