
using OrderManagement.Domain.Customers;
using OrderManagement.Domain.Orders;
using OrderManagement.SharedKernel;
using OrderManagement.SharedKernel.ValueObjects;

namespace OrderManagement.Domain.orders;

/// <summary>
/// Aggregate Root — Order.
/// Owns: OrderItem, OrderStatusHistory, Payment, Shipment.
/// References Customer / Product / Voucher by Id + snapshots only.
/// </summary>
public sealed class Order : Entity<Guid>, IAggregateRoot
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public Address ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public Money SubtotalAmount { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public Money ShippingFee { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public string Currency { get; private set; } = "VND";
    public Guid? VoucherId { get; private set; }
    public string? VoucherCode { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PlacedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    // Concurrency token
    public uint Version { get; private set; }   // chỉ cần khai báo, không cần [Timestamp], vì cấu hình Fluent API

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public Payment? Payment { get; private set; }
    public Shipment? Shipment { get; private set; }

    private Order() { }

    private Order(Guid id, Guid customerId, Address shippingAddress, string customerEmail) : base(id)
    {
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Order> CreateDraft(
        Guid customerId, Address shippingAddress, string customerEmail = "")
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>(OrderErrors.CustomerNotFound(customerId));
        if (shippingAddress is null)
            return Result.Failure<Order>(OrderErrors.ShippingAddressRequired);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Draft,
            SubtotalAmount = Money.ZeroOf("VND"),
            DiscountAmount = Money.ZeroOf("VND"),
            ShippingFee = Money.ZeroOf("VND"),
            TotalAmount = Money.ZeroOf("VND"),
            CreatedAt = DateTime.UtcNow
        };

        order.AddHistory(OrderStatus.Draft, OrderStatus.Draft, "Tạo đơn nháp");
        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, order.CustomerId, order.CustomerEmail));
        return order;
    }

    public void AddItem(
        Guid productId, string productName, string productSku,
        Money unitPrice, int quantity, decimal weightKg)
    {
        EnsureModifiable();
        if (quantity <= 0)
            throw new DomainException(OrderErrors.InvalidQuantity.Description);

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
            existing.IncreaseQuantity(quantity);
        else
            _items.Add(OrderItem.Create(Id, productId, productName, productSku, unitPrice, quantity, weightKg));

        RecalculateTotals();
        RaiseDomainEvent(new OrderItemAddedEvent(Id, productId, productName, quantity, unitPrice.Amount));
    }

    public void RemoveItem(Guid orderItemId)
    {
        EnsureModifiable();
        var item = _items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new NotFoundException(OrderErrors.ItemNotFound(orderItemId).Description);
        _items.Remove(item);
        RecalculateTotals();
    }

    public void UpdateItemQuantity(Guid orderItemId, int newQuantity)
    {
        EnsureModifiable();
        var item = _items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new NotFoundException(OrderErrors.ItemNotFound(orderItemId).Description);
        item.UpdateQuantity(newQuantity);
        RecalculateTotals();
    }

    /// <summary>
    /// Apply voucher by snapshot values (code, discount already calculated outside).
    /// Does not hold Voucher entity.
    /// </summary>
    public void ApplyVoucher(Guid voucherId, string voucherCode, Money discount)
    {
        EnsureModifiable();
        VoucherId = voucherId;
        VoucherCode = voucherCode;
        DiscountAmount = discount;
        RecalculateTotals();
        RaiseDomainEvent(new OrderVoucherAppliedEvent(Id, voucherId, voucherCode, discount.Amount, discount.Currency));
    }

    public void RemoveVoucher()
    {
        EnsureModifiable();
        VoucherId = null;
        VoucherCode = null;
        DiscountAmount = Money.ZeroOf(Currency);
        RecalculateTotals();
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        EnsureModifiable();
        if (newAddress is null)
            throw new DomainException(OrderErrors.ShippingAddressRequired.Description);
        ShippingAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes) { Notes = notes?.Trim(); UpdatedAt = DateTime.UtcNow; }

    public void SetShippingFee(Money fee)
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Placed or OrderStatus.Confirmed))
            throw new DomainException("Chỉ cập nhật phí ship ở giai đoạn sớm.");
        ShippingFee = fee;
        RecalculateTotals();
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException(OrderErrors.CannotPlace(Status).Description);
        if (!_items.Any())
            throw new DomainException(OrderErrors.EmptyItems.Description);

        ChangeStatus(OrderStatus.Placed, "Khách hàng đặt hàng");
        PlacedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderPlacedEvent(
            Id, CustomerId, CustomerEmail, TotalAmount.Amount, Currency,
            _items.Select(i => new OrderItemSnapshot(
                i.ProductId, i.ProductName, i.Quantity, i.UnitPrice.Amount)).ToList()));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Placed)
            throw new DomainException(OrderErrors.CannotConfirm(Status).Description);
        ChangeStatus(OrderStatus.Confirmed, "Đơn hàng được xác nhận");
        RaiseDomainEvent(new OrderConfirmedEvent(Id, CustomerId));
    }

    public void MarkAsPaid(Payment payment)
    {
        if (Status is not (OrderStatus.Placed or OrderStatus.Confirmed))
            throw new DomainException(OrderErrors.CannotMarkAsPaid(Status).Description);
        if (payment is null)
            throw new DomainException("Payment không được null.");

        Payment = payment;
        ChangeStatus(OrderStatus.Paid, "Thanh toán thành công");
        RaiseDomainEvent(new OrderPaidEvent(
            Id, CustomerId, payment.Id,
            payment.Amount.Amount, payment.Amount.Currency, payment.Method));
    }

    public void StartProcessing()
    {
        if (Status is not (OrderStatus.Paid or OrderStatus.Confirmed))
            throw new DomainException($"Không thể xử lý ở trạng thái {Status}.");
        ChangeStatus(OrderStatus.Processing, "Bắt đầu xử lý đơn hàng");
    }

    public void Ship(string trackingNumber, string carrier, ShippingZone zone, DateTime? estimatedDelivery = null)
    {
        if (Status is not (OrderStatus.Processing or OrderStatus.Paid or OrderStatus.Confirmed))
            throw new DomainException(OrderErrors.CannotShip(Status).Description);
        if (Shipment is not null)
            throw new DomainException(OrderErrors.AlreadyHasShipment.Description);

        var fee = zone.CalculateShippingFee(TotalWeightKg, Currency);
        Shipment = Shipment.Create(Id, ShippingAddress, zone, fee, estimatedDelivery);
        Shipment.AssignTracking(trackingNumber, carrier);
        Shipment.MarkAsPickedUp();

        ShippingFee = fee;
        RecalculateTotals();
        ChangeStatus(OrderStatus.Shipped, $"Giao {carrier} - {trackingNumber}");
        RaiseDomainEvent(new OrderShippedEvent(Id, CustomerId, CustomerEmail, trackingNumber, carrier, estimatedDelivery));
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException(OrderErrors.CannotDeliver(Status).Description);
        Shipment?.MarkAsDelivered();
        ChangeStatus(OrderStatus.Delivered, "Giao hàng thành công");
        RaiseDomainEvent(new OrderDeliveredEvent(Id, CustomerId));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException(OrderErrors.CannotComplete(Status).Description);
        ChangeStatus(OrderStatus.Completed, "Đơn hàng hoàn tất");
        RaiseDomainEvent(new OrderCompletedEvent(Id, CustomerId));
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Completed or OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.CannotCancel(Status).Description);
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(OrderErrors.CancellationReasonRequired.Description);

        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        ChangeStatus(OrderStatus.Cancelled, $"Hủy: {reason}");
        RaiseDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));
    }

    public void MarkAsReturned()
    {
        if (Status is not (OrderStatus.Delivered or OrderStatus.Completed))
            throw new DomainException(OrderErrors.CannotReturn(Status).Description);
        ChangeStatus(OrderStatus.Returned, "Đơn hàng đã trả lại");
    }

    public decimal TotalWeightKg => _items.Sum(i => i.TotalWeightKg);

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Aggregate(Money.ZeroOf(Currency), (s, i) => s.Add(i.Subtotal));
        var after = Math.Max(0, SubtotalAmount.Amount - DiscountAmount.Amount);
        TotalAmount = Money.Create(after, Currency).Add(ShippingFee);
        UpdatedAt = DateTime.UtcNow;
    }

    private void ChangeStatus(OrderStatus to, string? note = null)
    {
        var from = Status;
        Status = to;
        UpdatedAt = DateTime.UtcNow;
        AddHistory(from, to, note);
    }

    private void AddHistory(OrderStatus from, OrderStatus to, string? note = null)
        => _statusHistory.Add(OrderStatusHistory.Create(Id, from, to, note));

    private void EnsureModifiable()
    {
        if (Status is not OrderStatus.Draft)
            throw new DomainException(OrderErrors.NotModifiable(Status).Description);
    }
}
