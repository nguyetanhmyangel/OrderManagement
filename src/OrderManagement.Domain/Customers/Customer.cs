using OrderManagement.Domain.Enums;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Customers;

/// <summary>
/// Aggregate Root - Khách hàng.
/// Consistency boundary: profile, addresses, tier, loyalty points.
/// </summary>
public sealed class Customer : AggregateRoot
{
    public Guid Id { get; set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public CustomerTier Tier { get; private set; }
    public int LoyaltyPoints { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer() { }

    public static Result<Customer> Create(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber = null,
        Address? billingAddress = null,
        Address? shippingAddress = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Customer>(CustomerErrors.FirstNameRequired);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Customer>(CustomerErrors.LastNameRequired);

        Email emailVo;
        try
        {
            emailVo = Email.Create(email);
        }
        catch (DomainException)
        {
            return Result.Failure<Customer>(CustomerErrors.InvalidEmail);
        }

        PhoneNumber? phoneVo = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            try
            {
                phoneVo = PhoneNumber.Create(phoneNumber);
            }
            catch (DomainException)
            {
                return Result.Failure<Customer>(CustomerErrors.InvalidPhoneNumber);
            }
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = emailVo,
            PhoneNumber = phoneVo,
            BillingAddress = billingAddress,
            ShippingAddress = shippingAddress,
            Tier = CustomerTier.Standard,
            LoyaltyPoints = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // customer.RaiseDomainEvent(new CustomerCreatedEvent(
        //     customer.Id,
        //     customer.Email.Value,
        //     customer.GetFullName()));

        return customer;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public void UpdateContactInfo(string firstName, string lastName, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException(CustomerErrors.FirstNameRequired.Description);

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException(CustomerErrors.LastNameRequired.Description);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            PhoneNumber = PhoneNumber.Create(phoneNumber);

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        Email = Email.Create(email);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBillingAddress(Address address)
    {
        if (address is null)
            throw new DomainException("Địa chỉ thanh toán không được null.");

        BillingAddress = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateShippingAddress(Address address)
    {
        if (address is null)
            throw new DomainException("Địa chỉ giao hàng không được null.");

        ShippingAddress = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpgradeTier(CustomerTier newTier)
    {
        if (newTier <= Tier)
            throw new DomainException(
                CustomerErrors.CannotDowngradeTier(Tier.ToString(), newTier.ToString()).Description);

        var oldTier = Tier;
        Tier = newTier;
        UpdatedAt = DateTime.UtcNow;

        //RaiseDomainEvent(new CustomerTierUpgradedEvent(Id, oldTier, newTier));
    }

    public void AddLoyaltyPoints(int points)
    {
        if (points <= 0)
            throw new DomainException("Số điểm phải lớn hơn 0.");

        LoyaltyPoints += points;
        UpdatedAt = DateTime.UtcNow;

        //RaiseDomainEvent(new CustomerLoyaltyPointsChangedEvent(Id, points, LoyaltyPoints));
        RecalculateTier();
    }

    public void RedeemLoyaltyPoints(int points)
    {
        if (points <= 0)
            throw new DomainException("Số điểm phải lớn hơn 0.");

        if (LoyaltyPoints < points)
            throw new DomainException(
                CustomerErrors.InsufficientLoyaltyPoints(LoyaltyPoints, points).Description);

        LoyaltyPoints -= points;
        UpdatedAt = DateTime.UtcNow;

        //RaiseDomainEvent(new CustomerLoyaltyPointsChangedEvent(Id, -points, LoyaltyPoints));
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException(CustomerErrors.AlreadyActive(Id).Description);

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        //RaiseDomainEvent(new CustomerActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(CustomerErrors.AlreadyInactive(Id).Description);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        //RaiseDomainEvent(new CustomerDeactivatedEvent(Id));
    }

    private void RecalculateTier()
    {
        var newTier = LoyaltyPoints switch
        {
            >= 10000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Standard
        };

        if (newTier > Tier)
        {
            var oldTier = Tier;
            Tier = newTier;
            //RaiseDomainEvent(new CustomerTierUpgradedEvent(Id, oldTier, newTier));
        }
    }
}
