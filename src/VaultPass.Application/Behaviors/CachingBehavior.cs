using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace VaultPass.Application.Behaviors;

// Marker interface for queries that should be cached.
// The request must implement this to opt in to caching.
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}

public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Skip caching for requests that do not implement ICacheable
        if (request is not ICacheable cacheable)
            return await next();

        var cacheKey = cacheable.CacheKey;

        // Try to return a cached response before hitting the database
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT: {Key}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        // Cache miss — execute the handler and store the result
        var response = await next();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheable.CacheDuration ?? TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), options, ct);

        _logger.LogInformation("Cache SET: {Key}", cacheKey);
        return response;
    }
}
