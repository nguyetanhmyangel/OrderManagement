namespace OrderManagement.Infrastructure.Outbox;

internal sealed record OutboxMessageDto(
    Guid Id,
    string Type,
    string Content,
    int RetryCount);
