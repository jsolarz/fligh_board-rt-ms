using FlightBoard.Api.iFX.Contract.Service;
using Microsoft.Extensions.Caching.Memory;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Fallback memory cache service when Redis is not available
/// Simple implementation for development and single-instance scenarios
/// </summary>
public class FallbackMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FallbackMemoryCacheService> _logger;

    public FallbackMemoryCacheService(IMemoryCache memoryCache, ILogger<FallbackMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Memory cache hit: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Memory cache miss: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (value == null) return Task.CompletedTask;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };

        _memoryCache.Set(key, value, options);
        _logger.LogDebug("Memory cache set: {Key}", key);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Memory cache removed: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Pattern removal not supported in memory cache: {Pattern}", pattern);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var exists = _memoryCache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null) return cachedValue;

        var newValue = await factory();
        if (newValue != null)
        {
            await SetAsync(key, newValue, expiration, cancellationToken);
        }

        return newValue;
    }

    public Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new Dictionary<string, object>
            {
                ["CacheType"] = "Memory Cache",
                ["TotalKeys"] = 0, // MemoryCache doesn't expose key count
                ["TotalHits"] = 0, // Would need custom tracking
                ["TotalMisses"] = 0, // Would need custom tracking
                ["LastUpdated"] = DateTime.UtcNow,
                ["MemoryCacheEnabled"] = true
            };

            return Task.FromResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory cache statistics");
            return Task.FromResult(new Dictionary<string, object>
            {
                ["Error"] = ex.Message,
                ["CacheType"] = "Memory Cache Error"
            });
        }
    }

    public Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Clearing all memory cache data");

            // Note: MemoryCache doesn't have a built-in clear all method
            // In a production implementation, you'd need to track keys or use a custom wrapper
            if (_memoryCache is MemoryCache memCache)
            {
                // This would require reflection or a custom implementation
                _logger.LogWarning("Memory cache clear all requires custom implementation");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all memory cache");
            throw;
        }
    }
}
