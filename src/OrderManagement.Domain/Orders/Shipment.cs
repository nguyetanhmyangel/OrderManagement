

using OrderManagement.Domain.Customers;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.Orders;

/// <summary>Entity owned by Order Aggregate.</summary>
public sealed class Shipment : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? Carrier { get; private set; }
    public string ShippingZoneCode { get; private set; } = null!;
    public Money ShippingFee { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public ShipmentStatus Status { get; private set; }
    public DateTime? EstimatedDelivery { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Shipment() { }

    private Shipment(Guid id, Guid orderId,
        Address shippingAddress,
        ShippingZone zone,
        Money shippingFee,
        DateTime? estimatedDelivery) : base(id)
    {
        OrderId = orderId;
        ShippingAddress = shippingAddress;
        ShippingZoneCode = zone.Code;
        ShippingFee = shippingFee;
        Status = ShipmentStatus.Pending;
        EstimatedDelivery = estimatedDelivery;
        CreatedAt = DateTime.UtcNow;
    }

    internal static Shipment Create(
        Guid orderId,
        Address shippingAddress,
        ShippingZone zone,
        Money shippingFee,
        DateTime? estimatedDelivery = null)
    {
        if (shippingAddress is null)
            throw new DomainException("Địa chỉ giao hàng không được null.");
        if (zone is null)
            throw new DomainException("Shipping zone không được null.");

        return new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ShippingAddress = shippingAddress,
            ShippingZoneCode = zone.Code,
            ShippingFee = shippingFee,
            Status = ShipmentStatus.Pending,
            EstimatedDelivery = estimatedDelivery,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignTracking(string trackingNumber, string carrier)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException("Mã tracking không được để trống.");
        if (string.IsNullOrWhiteSpace(carrier))
            throw new DomainException("Đơn vị vận chuyển không được để trống.");

        TrackingNumber = trackingNumber.Trim();
        Carrier = carrier.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPickedUp()
    {
        EnsureTransition(ShipmentStatus.PickedUp);
        Status = ShipmentStatus.PickedUp;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsInTransit()
    {
        EnsureTransition(ShipmentStatus.InTransit);
        Status = ShipmentStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOutForDelivery()
    {
        EnsureTransition(ShipmentStatus.OutForDelivery);
        Status = ShipmentStatus.OutForDelivery;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        EnsureTransition(ShipmentStatus.Delivered);
        Status = ShipmentStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (Status == ShipmentStatus.Delivered)
            throw new DomainException("Không thể failed shipment đã delivered.");
        Status = ShipmentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReturnedToSender()
    {
        if (Status is not (ShipmentStatus.Failed or ShipmentStatus.OutForDelivery))
            throw new DomainException("Chỉ shipment failed/out-for-delivery mới trả sender.");
        Status = ShipmentStatus.ReturnedToSender;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureTransition(ShipmentStatus target)
    {
        var ok = Status switch
        {
            ShipmentStatus.Pending => target is ShipmentStatus.PickedUp,
            ShipmentStatus.PickedUp => target is ShipmentStatus.InTransit,
            ShipmentStatus.InTransit => target is ShipmentStatus.OutForDelivery or ShipmentStatus.Failed,
            ShipmentStatus.OutForDelivery => target is ShipmentStatus.Delivered or ShipmentStatus.Failed,
            _ => false
        };
        if (!ok)
            throw new DomainException($"Không thể chuyển shipment từ {Status} sang {target}.");
    }
}
