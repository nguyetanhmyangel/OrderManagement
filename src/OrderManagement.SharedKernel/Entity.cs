namespace OrderManagement.SharedKernel;

public abstract class Entity<TKey> : IEntity where TKey : notnull
{
    public TKey Id { get; protected set; }

    protected Entity(TKey id) { Id = id; }
    protected Entity() { } // Dành cho Dapper/ORM

    private readonly List<IDomainEvent> _domainEvents = new();

    // Triển khai từ IEntity
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

/*
 * Table có Khóa chính >= 2 trường, ví dụ OrderDetail
    // 1. Định nghĩa Composite Key dưới dạng Value Object (struct hoặc record)
   public readonly record struct OrderDetailId(Guid OrderId, Guid ProductId);
   // 2. Sử dụng cho Entity có 2 khóa chính:
   public class OrderDetail : Entity<OrderDetailId>
   {
       public int Quantity { get; private set; }
       public decimal UnitPrice { get; private set; }

       // Dapper map các cột OrderId, ProductId vào đây
       public OrderDetail(Guid orderId, Guid productId, int quantity, decimal unitPrice)
           : base(new OrderDetailId(orderId, productId))
       {
           Quantity = quantity;
           UnitPrice = unitPrice;
       }
   }
 */
