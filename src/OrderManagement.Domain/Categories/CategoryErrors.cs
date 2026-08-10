using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) => Error.NotFound(
        "Categories.NotFound",
        $"The category with Id = '{categoryId}' was not found.");

    public static Error NameRequired => Error.Validation(
        "Categories.NameRequired",
        "Tên danh mục không được để trống.");

    public static Error CannotBeOwnParent => Error.Problem(
        "Categories.CannotBeOwnParent",
        "Danh mục không thể là parent của chính nó.");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        "Categories.NameAlreadyExists",
        $"Category name '{name}' already exists.");
}
