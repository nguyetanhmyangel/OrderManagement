using OrderManagement.Infrastructure.Outbox;

namespace OrderManagement.Infrastructure.outbox;


public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public DateTime? FailedOnUtc { get; set; }
    public string Status { get; set; } = OutboxStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? NextAttemptOnUtc { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public string? LockedBy { get; set; }
    public string? LastError { get; set; }
}
