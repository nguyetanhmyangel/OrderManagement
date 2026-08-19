using OrderManagement.Domain.Products;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Events;
/// <summary>
/// Dùng để map giữa Domain Event Type ↔ tên ổn định (event versioning), tránh dùng AssemblyQualifiedName (dễ gãy khi rename class/namespace).
/// </summary>
public sealed class EventTypeRegistry : IEventTypeRegistry
{
    private static readonly Dictionary<string, Type> NameToType = new();
    private static readonly Dictionary<Type, string> TypeToName = new();

    static EventTypeRegistry()
    {
        Register<ProductPriceChangedEvent>("Product.PriceChanged.v1");
    }

    private static void Register<T>(string name) where T : IDomainEvent
    {
        NameToType[name] = typeof(T);
        TypeToName[typeof(T)] = name;
    }

    public string GetEventName(Type eventType) =>
        TypeToName.TryGetValue(eventType, out var name)
            ? name
            : throw new InvalidOperationException($"Event chưa được đăng ký: {eventType.Name}");

    public Type GetEventType(string eventName) =>
        NameToType.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException($"Tên Event không hợp lệ: {eventName}");
}
