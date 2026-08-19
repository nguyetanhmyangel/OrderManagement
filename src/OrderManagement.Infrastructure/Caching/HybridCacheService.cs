using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Abstractions.Caching;
using OrderManagement.Application.Abstractions.Messaging;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Caching;

internal sealed class CacheQueryDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ICacheService cache,
    ILogger<CacheQueryDecorator<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>, ICacheableQuery
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = query.CacheKey;

        var cached = await cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                logger.LogDebug("Cache miss: {CacheKey}", cacheKey);
                var result = await inner.Handle(query, token);
                return result.IsSuccess ? result.Value : default;
            },
            query.Expiration,
            cancellationToken);

        if (cached is null)
            return Result.Failure<TResponse>(Error.NotFound("Cache.Miss", "Không tìm thấy dữ liệu."));

        return Result.Success(cached);
    }
}
