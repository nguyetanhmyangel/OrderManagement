namespace OrderManagement.Infrastructure.Events;

public interface IEventTypeRegistry
{
    string GetEventName(Type eventType);
    Type GetEventType(string eventName);
}
