namespace OrderManagement.SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
