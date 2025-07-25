using Microsoft.Extensions.Caching.Memory;
using FlightBoard.Api.iFX.Contract.Service;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Memory caching service implementation
/// Following iDesign Method: Infrastructure framework service implementation
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, bool> _keyTracker;
    private long _hitCount;
    private long _missCount;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _keyTracker = new ConcurrentDictionary<string, bool>();
    }

    /// <summary>
    /// Gets a cached item by key
    /// </summary>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = _memoryCache.Get<T>(key);
        if (value != null)
        {
            Interlocked.Increment(ref _hitCount);
        }
        else
        {
            Interlocked.Increment(ref _missCount);
        }
        return Task.FromResult(value);
    }

    /// <summary>
    /// Sets a cached item with expiration
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
            SlidingExpiration = null, // Use absolute expiration
            Priority = CacheItemPriority.Normal
        };

        // Add removal callback to clean up key tracker
        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
        {
            EvictionCallback = (key, value, reason, state) =>
            {
                _keyTracker.TryRemove(key.ToString()!, out _);
            }
        });

        _memoryCache.Set(key, value, options);
        _keyTracker.TryAdd(key, true);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes an item from cache
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _keyTracker.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes multiple items from cache by pattern (simplified pattern matching)
    /// </summary>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.IgnoreCase);
        var keysToRemove = _keyTracker.Keys
            .Where(key => regex.IsMatch(key))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_keyTracker.ContainsKey(key));
    }

    /// <summary>
    /// Get or set cache value with factory function (cache-aside pattern)
    /// </summary>
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var value = await GetAsync<T>(key, cancellationToken);
        if (value != null)
        {
            return value;
        }

        value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    /// <summary>
    /// Get cache statistics for performance monitoring
    /// </summary>
    public Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new Dictionary<string, object>
        {
            { "provider", "Memory" },
            { "total_keys", _keyTracker.Count },
            { "hit_count", _hitCount },
            { "miss_count", _missCount },
            { "hit_ratio", _hitCount + _missCount > 0 ? (double)_hitCount / (_hitCount + _missCount) : 0.0 }
        };

        return Task.FromResult(stats);
    }

    /// <summary>
    /// Clear all cached data (admin operation)
    /// </summary>
    public Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        // Memory cache doesn't have a direct clear method, so we remove all tracked keys
        var keysToRemove = _keyTracker.Keys.ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);
        }

        // Reset counters
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);

        return Task.CompletedTask;
    }
}
