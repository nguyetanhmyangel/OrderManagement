
using OrderManagement.SharedKernel;

namespace OrderManagement.Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(Guid customerId) => Error.NotFound(
        "Customers.NotFound",
        $"The customer with Id = '{customerId}' was not found.");

    public static Error FirstNameRequired => Error.Validation(
        "Customers.FirstNameRequired",
        "Họ không được để trống.");

    public static Error LastNameRequired => Error.Validation(
        "Customers.LastNameRequired",
        "Tên không được để trống.");

    public static Error EmailRequired => Error.Validation(
        "Customers.EmailRequired",
        "Email không được để trống.");

    public static Error InvalidEmail => Error.Validation(
        "Customers.InvalidEmail",
        "Email không hợp lệ.");

    public static Error InvalidPhoneNumber => Error.Validation(
        "Customers.InvalidPhoneNumber",
        "Số điện thoại không hợp lệ.");

    public static Error AlreadyActive(Guid customerId) => Error.Problem(
        "Customers.AlreadyActive",
        $"The customer with Id = '{customerId}' is already active.");

    public static Error AlreadyInactive(Guid customerId) => Error.Problem(
        "Customers.AlreadyInactive",
        $"The customer with Id = '{customerId}' is already inactive.");

    public static Error CannotDowngradeTier(string currentTier, string newTier) => Error.Problem(
        "Customers.CannotDowngradeTier",
        $"Cannot downgrade or set the same tier. Current: {currentTier}, New: {newTier}.");

    public static Error InsufficientLoyaltyPoints(int current, int required) => Error.Problem(
        "Customers.InsufficientLoyaltyPoints",
        $"Không đủ điểm. Hiện có: {current}, cần: {required}.");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "Customers.EmailAlreadyExists",
        $"Email '{email}' is already registered.");
}
