namespace OrderManagement.Application.Abstractions.Caching;

// Marker interface – Query nào muốn cache thì implement
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
