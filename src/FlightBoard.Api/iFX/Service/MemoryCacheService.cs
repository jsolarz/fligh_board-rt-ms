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

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _keyTracker = new ConcurrentDictionary<string, bool>();
    }

    /// <summary>
    /// Gets a cached item by key
    /// </summary>
    public Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = _memoryCache.Get<T>(key);
        return Task.FromResult(value);
    }

    /// <summary>
    /// Sets a cached item with expiration
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry,
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
    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _keyTracker.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes multiple items from cache by pattern (simplified pattern matching)
    /// </summary>
    public Task RemoveByPatternAsync(string pattern)
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
    public bool Exists(string key)
    {
        return _keyTracker.ContainsKey(key);
    }
}
