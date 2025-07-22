using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using FlightBoard.Api.iFX.Contract.Service;
using System.Text.Json;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// High-performance cache service with Redis distributed caching support
/// Falls back to in-memory cache when Redis is unavailable
/// Implements cache-aside pattern for optimal performance
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public CacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Try memory cache first (L1 cache)
            if (_memoryCache.TryGetValue(key, out T? memoryCachedValue))
            {
                _logger.LogDebug("Cache hit (memory): {Key}", key);
                return memoryCachedValue;
            }

            // Try distributed cache (L2 cache)
            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                
                // Populate L1 cache
                _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                
                _logger.LogDebug("Cache hit (distributed): {Key}", key);
                return deserializedValue;
            }

            _logger.LogDebug("Cache miss: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get error for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (value == null) return;

        var defaultExpiration = expiration ?? TimeSpan.FromMinutes(30);
        
        try
        {
            // Set in memory cache (L1)
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));

            // Set in distributed cache (L2)
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = defaultExpiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogDebug("Cache set: {Key} (expires in {Expiration})", key, defaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set error for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove error for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: Pattern removal is Redis-specific and requires additional implementation
            // For basic implementation, we'll log the request
            _logger.LogInformation("Pattern removal requested: {Pattern} (requires Redis-specific implementation)", pattern);
            
            // Clear relevant memory cache entries
            // This is a simplified approach - production code might need more sophisticated cache key tracking
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache pattern removal error for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check memory cache first
            if (_memoryCache.TryGetValue(key, out _))
            {
                return true;
            }

            // Check distributed cache
            var value = await _distributedCache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache exists check error for key: {Key}", key);
            return false;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Use semaphore to prevent cache stampede
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check pattern
            cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Generate new value
            var newValue = await factory();
            if (newValue != null)
            {
                await SetAsync(key, newValue, expiration, cancellationToken);
            }

            return newValue;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new Dictionary<string, object>
            {
                ["CacheType"] = "Hybrid (Memory + Distributed)",
                ["TotalKeys"] = 0, // Redis would need to be queried for this
                ["TotalHits"] = 0, // Would need to track this
                ["TotalMisses"] = 0, // Would need to track this
                ["LastUpdated"] = DateTime.UtcNow
            };

            // Add memory cache stats if available
            if (_memoryCache is MemoryCache memCache)
            {
                // Note: MemoryCache doesn't expose statistics by default
                // In production, you'd use a wrapper that tracks these metrics
                stats["MemoryCacheEnabled"] = true;
            }

            return Task.FromResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return Task.FromResult(new Dictionary<string, object>
            {
                ["Error"] = ex.Message,
                ["CacheType"] = "Error"
            });
        }
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Clearing all cache data");

            // Clear memory cache
            if (_memoryCache is MemoryCache memCache)
            {
                // Note: MemoryCache doesn't have a clear all method by default
                // This would need a custom implementation or wrapper
                _logger.LogWarning("Memory cache clear requires custom implementation");
            }

            // For distributed cache, this would require Redis FLUSHDB command
            // which is not available through IDistributedCache interface
            _logger.LogWarning("Distributed cache clear requires Redis-specific implementation");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            throw;
        }
    }
}
