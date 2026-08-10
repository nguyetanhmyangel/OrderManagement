using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid productId) => Error.NotFound(
        "Products.NotFound",
        $"The product with Id = '{productId}' was not found.");

    public static Error SkuRequired => Error.Validation(
        "Products.SkuRequired",
        "SKU không được để trống.");

    public static Error NameRequired => Error.Validation(
        "Products.NameRequired",
        "Tên sản phẩm không được để trống.");

    public static Error InvalidPrice => Error.Validation(
        "Products.InvalidPrice",
        "Giá sản phẩm phải lớn hơn 0.");

    public static Error InvalidWeight => Error.Validation(
        "Products.InvalidWeight",
        "Trọng lượng phải lớn hơn 0.");

    public static Error InvalidCompareAtPrice => Error.Validation(
        "Products.InvalidCompareAtPrice",
        "Giá so sánh (compare-at) phải lớn hơn hoặc bằng giá bán.");

    public static Error SkuAlreadyExists(string sku) => Error.Conflict(
        "Products.SkuAlreadyExists",
        $"SKU '{sku}' already exists.");

    public static Error AlreadyActive(Guid productId) => Error.Problem(
        "Products.AlreadyActive",
        $"The product with Id = '{productId}' is already active.");

    public static Error AlreadyInactive(Guid productId) => Error.Problem(
        "Products.AlreadyInactive",
        $"The product with Id = '{productId}' is already inactive.");

    public static Error ImageNotFound(Guid imageId) => Error.NotFound(
        "Products.ImageNotFound",
        $"The product image with Id = '{imageId}' was not found.");

    public static Error ImageUrlRequired => Error.Validation(
        "Products.ImageUrlRequired",
        "URL hình ảnh không được để trống.");
}
