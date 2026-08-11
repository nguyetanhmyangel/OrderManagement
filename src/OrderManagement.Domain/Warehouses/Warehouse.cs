using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Warehouses;

/// <summary>Aggregate Root — Warehouse.</summary>
public sealed class Warehouse : Entity<Guid>, IAggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Warehouse() { }

    private Warehouse(Guid id, string code, string name, Address address, bool isActive, bool isDefault,
        DateTime createdAt) : base(id)
    {
        Code = code;
        Name = name;
        Address = address;
        IsActive = isActive;
        IsDefault = isDefault;
        CreatedAt = createdAt;
    }

    public static Warehouse Create(string code, string name, Address address, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(WarehouseErrors.CodeRequired.Description);
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(WarehouseErrors.NameRequired.Description);
        if (address is null)
            throw new DomainException(WarehouseErrors.AddressRequired.Description);

        var wh = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Address = address,
            IsActive = true,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };
        wh.RaiseDomainEvent(new WarehouseCreatedEvent(wh.Id, wh.Code, wh.Name));
        return wh;
    }

    public void Update(string name, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(WarehouseErrors.NameRequired.Description);
        if (address is null)
            throw new DomainException(WarehouseErrors.AddressRequired.Description);
        Name = name.Trim();
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault() { IsDefault = true; UpdatedAt = DateTime.UtcNow; }
    public void UnsetDefault() { IsDefault = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }

    public void Deactivate()
    {
        if (IsDefault)
            throw new DomainException(WarehouseErrors.CannotDeactivateDefault.Description);
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WarehouseDeactivatedEvent(Id, Code));
    }
}
