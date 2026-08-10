namespace OrderManagement.SharedKernel;

/// <summary>
/// Chỉ là marker — không bắt buộc dùng.
/// Nếu không muốn AggregateRoot, Customer : Entity là đủ.
/// </summary>
public interface IAggregateRoot
{
    // Thường để trống (Marker Interface)
}
