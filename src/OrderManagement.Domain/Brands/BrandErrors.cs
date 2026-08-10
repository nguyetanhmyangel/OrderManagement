
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Brands;

public static class BrandErrors
{
    public static Error NotFound(Guid brandId) => Error.NotFound(
        "Brands.NotFound",
        $"The brand with Id = '{brandId}' was not found.");

    public static Error NameRequired => Error.Validation(
        "Brands.NameRequired",
        "Tên thương hiệu không được để trống.");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        "Brands.NameAlreadyExists",
        $"Brand name '{name}' already exists.");
}
