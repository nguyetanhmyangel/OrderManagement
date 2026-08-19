namespace OrderManagement.Infrastructure.Outbox;

public static class OutboxStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Processed = "Processed";
    public const string Failed = "Failed";

    public static readonly string[] All = [Pending, Processing, Processed, Failed];
}
