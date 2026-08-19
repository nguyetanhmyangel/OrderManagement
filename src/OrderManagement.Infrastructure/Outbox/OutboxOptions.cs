namespace OrderManagement.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public int BatchSize { get; set; } = 50;
    public int PollingIntervalSeconds { get; set; } = 2;
    public int MaxRetryCount { get; set; } = 5;
    public int LockTimeoutSeconds { get; set; } = 60;
    public int MaxDegreeOfParallelism { get; set; } = 4;
}
