using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using FlightBoard.Api.iFX.Contract.Service;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

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
    private readonly IRedisService _redisService;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile bool _redisAvailable = true;
    
    // Track memory cache keys for pattern removal
    private readonly ConcurrentDictionary<string, bool> _memoryKeyTracker = new();

    public CacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        IRedisService redisService,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _redisService = redisService;
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

            // Try Redis only if we think it's available
            if (_redisAvailable)
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(500)); // Very short timeout

                    var distributedValue = await _distributedCache.GetStringAsync(key, timeoutCts.Token);
                    if (!string.IsNullOrEmpty(distributedValue))
                    {
                        var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                        
                        // Populate L1 cache for faster access next time
                        _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                        
                        _logger.LogDebug("Cache hit (Redis): {Key}", key);
                        return deserializedValue;
                    }
                }
                catch (Exception redisEx)
                {
                    _redisAvailable = false;
                    _logger.LogWarning(redisEx, "Redis cache failed for key: {Key} - switching to memory-only mode", key);
                    
                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
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
            // Set in memory cache with key tracking
            var memoryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (k, v, reason, state) =>
                        {
                            _memoryKeyTracker.TryRemove(k.ToString()!, out _);
                        }
                    }
                }
            };
            
            _memoryCache.Set(key, value, memoryOptions);
            _memoryKeyTracker.TryAdd(key, true);
            _logger.LogDebug("Cache set (memory): {Key}", key);

            // Try Redis only if we think it's available
            if (_redisAvailable)
            {
                try
                {
                    var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = defaultExpiration
                    };

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(500)); // Very short timeout

                    await _distributedCache.SetStringAsync(key, serializedValue, options, timeoutCts.Token);
                    _logger.LogDebug("Cache set (Redis): {Key}", key);
                }
                catch (Exception redisEx)
                {
                    _redisAvailable = false;
                    _logger.LogWarning(redisEx, "Redis cache failed for key: {Key} - switching to memory-only mode", key);
                    
                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache set error for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove from memory cache and tracking
            _memoryCache.Remove(key);
            _memoryKeyTracker.TryRemove(key, out _);
            
            if (_redisAvailable)
            {
                try
                {
                    await _distributedCache.RemoveAsync(key, cancellationToken);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Redis cache remove failed for key: {Key}", key);
                    _redisAvailable = false;

                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }
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
            var totalRemoved = 0;
            
            // Remove from memory cache using pattern matching
            var memoryRemoved = RemoveFromMemoryCacheByPattern(pattern);
            totalRemoved += memoryRemoved;
            
            // Remove from Redis using the Redis service
            if (_redisAvailable)
            {
                try
                {
                    var redisRemoved = await _redisService.RemoveByPatternAsync(pattern, cancellationToken);
                    totalRemoved += redisRemoved;
                    _logger.LogInformation("Removed {RedisCount} keys from Redis for pattern: {Pattern}", redisRemoved, pattern);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Redis pattern removal failed for pattern: {Pattern}", pattern);
                    _redisAvailable = false;
                    
                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }
            
            _logger.LogInformation("Pattern removal completed: {Pattern} - Total removed: {TotalCount} (Memory: {MemoryCount})", 
                pattern, totalRemoved, memoryRemoved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache pattern removal error for pattern: {Pattern}", pattern);
        }
    }

    private int RemoveFromMemoryCacheByPattern(string pattern)
    {
        try
        {
            // Convert pattern to regex (handle wildcards)
            var regexPattern = "^" + pattern.Replace("*", ".*").Replace("?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
            var keysToRemove = _memoryKeyTracker.Keys
                .Where(key => regex.IsMatch(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _memoryKeyTracker.TryRemove(key, out _);
            }

            _logger.LogDebug("Removed {Count} keys from memory cache for pattern: {Pattern}", keysToRemove.Count, pattern);
            return keysToRemove.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing memory cache keys by pattern: {Pattern}", pattern);
            return 0;
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
            if (_redisAvailable)
            {
                try
                {
                    var value = await _distributedCache.GetStringAsync(key, cancellationToken);
                    return !string.IsNullOrEmpty(value);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Redis cache exists check failed for key: {Key}", key);
                    _redisAvailable = false;

                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }

            return false;
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

    public async Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new Dictionary<string, object>
            {
                ["CacheType"] = "Hybrid (Memory + Distributed)",
                ["MemoryKeys"] = _memoryKeyTracker.Count,
                ["RedisAvailable"] = _redisAvailable,
                ["LastUpdated"] = DateTime.UtcNow
            };

            // Add Redis-specific stats if available
            if (_redisAvailable)
            {
                try
                {
                    var redisInfo = await _redisService.GetInfoAsync();
                    stats["RedisInfo"] = redisInfo;
                }
                catch (Exception ex)
                {
                    stats["RedisInfoError"] = ex.Message;
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return new Dictionary<string, object>
            {
                ["Error"] = ex.Message,
                ["CacheType"] = "Error"
            };
        }
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Clearing all cache data");

            var totalCleared = 0;

            // Clear memory cache
            var memoryKeys = _memoryKeyTracker.Keys.ToList();
            foreach (var key in memoryKeys)
            {
                _memoryCache.Remove(key);
                _memoryKeyTracker.TryRemove(key, out _);
            }
            totalCleared += memoryKeys.Count;

            // Clear Redis cache if available
            if (_redisAvailable)
            {
                try
                {
                    // Use Redis service to clear all keys with FlightBoard prefix
                    var redisCleared = await _redisService.RemoveByPatternAsync("FlightBoard:*", cancellationToken);
                    totalCleared += redisCleared;
                    _logger.LogInformation("Cleared {RedisCount} keys from Redis", redisCleared);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Failed to clear Redis cache");
                }
            }

            _logger.LogInformation("Cache clear completed - Total cleared: {TotalCount} (Memory: {MemoryCount})", 
                totalCleared, memoryKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            throw;
        }
    }
}
