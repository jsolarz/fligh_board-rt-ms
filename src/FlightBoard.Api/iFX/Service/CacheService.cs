using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using FlightBoard.Api.iFX.Contract.Service;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// High-performance cache service with Redis distributed caching support
/// Falls back to in-memory cache when Redis is unavailable
/// Implements cache-aside pattern for optimal performance with comprehensive statistics tracking
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly IRedisService _redisService;
    private readonly ICacheStatisticsTracker _statisticsTracker;
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
        ICacheStatisticsTracker statisticsTracker,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _redisService = redisService;
        _statisticsTracker = statisticsTracker;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Try memory cache first (L1 cache)
            if (_memoryCache.TryGetValue(key, out T? memoryCachedValue))
            {
                stopwatch.Stop();
                _statisticsTracker.TrackHit(key, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Memory);
                _logger.LogDebug("Cache hit (memory): {Key}", key);
                return memoryCachedValue;
            }

            var memoryMissTime = stopwatch.Elapsed.TotalMilliseconds;
            _statisticsTracker.TrackMiss(key, memoryMissTime, CacheLayer.Memory);

            // Try Redis only if we think it's available
            if (_redisAvailable)
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(500)); // Very short timeout

                    var redisStopwatch = Stopwatch.StartNew();
                    var distributedValue = await _distributedCache.GetStringAsync(key, timeoutCts.Token);
                    redisStopwatch.Stop();

                    if (!string.IsNullOrEmpty(distributedValue))
                    {
                        var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                        
                        // Populate L1 cache for faster access next time
                        var valueSize = System.Text.Encoding.UTF8.GetByteCount(distributedValue);
                        _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                        
                        stopwatch.Stop();
                        _statisticsTracker.TrackHit(key, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
                        _statisticsTracker.TrackHit(key, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Combined);
                        _statisticsTracker.TrackSet(key, 0, CacheLayer.Memory, valueSize); // Backfill to memory
                        
                        _logger.LogDebug("Cache hit (Redis): {Key}", key);
                        return deserializedValue;
                    }
                    else
                    {
                        _statisticsTracker.TrackMiss(key, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
                    }
                }
                catch (Exception redisEx)
                {
                    stopwatch.Stop();
                    _redisAvailable = false;
                    _statisticsTracker.TrackMiss(key, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
                    _logger.LogWarning(redisEx, "Redis cache failed for key: {Key} - switching to memory-only mode", key);
                    
                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }

            stopwatch.Stop();
            _statisticsTracker.TrackMiss(key, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Combined);
            _logger.LogDebug("Cache miss: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _statisticsTracker.TrackMiss(key, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Combined);
            _logger.LogWarning(ex, "Cache get error for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (value == null) return;

        var defaultExpiration = expiration ?? TimeSpan.FromMinutes(30);
        var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
        var valueSize = System.Text.Encoding.UTF8.GetByteCount(serializedValue);
        
        try
        {
            // Set in memory cache with key tracking
            var memoryStopwatch = Stopwatch.StartNew();
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
            memoryStopwatch.Stop();
            
            _statisticsTracker.TrackSet(key, memoryStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Memory, valueSize);
            _logger.LogDebug("Cache set (memory): {Key}", key);

            // Try Redis only if we think it's available
            if (_redisAvailable)
            {
                try
                {
                    var redisStopwatch = Stopwatch.StartNew();
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = defaultExpiration
                    };

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(500)); // Very short timeout

                    await _distributedCache.SetStringAsync(key, serializedValue, options, timeoutCts.Token);
                    redisStopwatch.Stop();
                    
                    _statisticsTracker.TrackSet(key, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis, valueSize);
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

            _statisticsTracker.TrackSet(key, 0, CacheLayer.Combined, valueSize);
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
            var memoryStopwatch = Stopwatch.StartNew();
            _memoryCache.Remove(key);
            _memoryKeyTracker.TryRemove(key, out _);
            memoryStopwatch.Stop();
            
            _statisticsTracker.TrackRemove(key, memoryStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Memory);
            
            if (_redisAvailable)
            {
                try
                {
                    var redisStopwatch = Stopwatch.StartNew();
                    await _distributedCache.RemoveAsync(key, cancellationToken);
                    redisStopwatch.Stop();
                    
                    _statisticsTracker.TrackRemove(key, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Redis cache remove failed for key: {Key}", key);
                    _redisAvailable = false;

                    // Schedule Redis availability check after 30 seconds
                    _ = Task.Delay(30000, cancellationToken).ContinueWith(_ => _redisAvailable = true, cancellationToken);
                }
            }
            
            _statisticsTracker.TrackRemove(key, 0, CacheLayer.Combined);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove error for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var totalRemoved = 0;
            
            // Remove from memory cache using pattern matching
            var memoryStopwatch = Stopwatch.StartNew();
            var memoryRemoved = RemoveFromMemoryCacheByPattern(pattern);
            memoryStopwatch.Stop();
            totalRemoved += memoryRemoved;
            
            _statisticsTracker.TrackPatternOperation(pattern, memoryRemoved, memoryStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Memory);
            
            // Remove from Redis using the Redis service
            if (_redisAvailable)
            {
                try
                {
                    var redisStopwatch = Stopwatch.StartNew();
                    var redisRemoved = await _redisService.RemoveByPatternAsync(pattern, cancellationToken);
                    redisStopwatch.Stop();
                    totalRemoved += redisRemoved;
                    
                    _statisticsTracker.TrackPatternOperation(pattern, redisRemoved, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
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
            
            stopwatch.Stop();
            _statisticsTracker.TrackPatternOperation(pattern, totalRemoved, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Combined);
            
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
            // Get comprehensive statistics from tracker
            var cacheStats = _statisticsTracker.GetStatistics();
            
            var stats = new Dictionary<string, object>
            {
                ["CacheType"] = "Hybrid (Memory + Distributed)",
                ["StartTime"] = cacheStats.StartTime,
                ["Uptime"] = cacheStats.Uptime,
                ["UptimeHours"] = Math.Round(cacheStats.Uptime.TotalHours, 2),
                ["RedisAvailable"] = _redisAvailable,
                ["LastUpdated"] = DateTime.UtcNow,
                
                // Memory layer statistics
                ["Memory"] = new Dictionary<string, object>
                {
                    ["TotalHits"] = cacheStats.Memory.TotalHits,
                    ["TotalMisses"] = cacheStats.Memory.TotalMisses,
                    ["TotalSets"] = cacheStats.Memory.TotalSets,
                    ["TotalRemoves"] = cacheStats.Memory.TotalRemoves,
                    ["TotalRequests"] = cacheStats.Memory.TotalRequests,
                    ["HitRatePercent"] = cacheStats.Memory.HitRatePercent,
                    ["AverageResponseTimeMs"] = cacheStats.Memory.AverageResponseTimeMs,
                    ["MinResponseTimeMs"] = cacheStats.Memory.MinResponseTimeMs,
                    ["MaxResponseTimeMs"] = cacheStats.Memory.MaxResponseTimeMs,
                    ["CurrentKeyCount"] = cacheStats.Memory.CurrentKeyCount,
                    ["TotalBytesStored"] = cacheStats.Memory.TotalBytesStored,
                    ["Operations"] = cacheStats.Memory.OperationStats
                },
                
                // Redis layer statistics
                ["Redis"] = new Dictionary<string, object>
                {
                    ["TotalHits"] = cacheStats.Redis.TotalHits,
                    ["TotalMisses"] = cacheStats.Redis.TotalMisses,
                    ["TotalSets"] = cacheStats.Redis.TotalSets,
                    ["TotalRemoves"] = cacheStats.Redis.TotalRemoves,
                    ["TotalRequests"] = cacheStats.Redis.TotalRequests,
                    ["HitRatePercent"] = cacheStats.Redis.HitRatePercent,
                    ["AverageResponseTimeMs"] = cacheStats.Redis.AverageResponseTimeMs,
                    ["MinResponseTimeMs"] = cacheStats.Redis.MinResponseTimeMs,
                    ["MaxResponseTimeMs"] = cacheStats.Redis.MaxResponseTimeMs,
                    ["TotalBytesStored"] = cacheStats.Redis.TotalBytesStored,
                    ["Operations"] = cacheStats.Redis.OperationStats
                },
                
                // Combined statistics
                ["Combined"] = new Dictionary<string, object>
                {
                    ["TotalHits"] = cacheStats.Combined.TotalHits,
                    ["TotalMisses"] = cacheStats.Combined.TotalMisses,
                    ["TotalSets"] = cacheStats.Combined.TotalSets,
                    ["TotalRemoves"] = cacheStats.Combined.TotalRemoves,
                    ["TotalRequests"] = cacheStats.Combined.TotalRequests,
                    ["HitRatePercent"] = cacheStats.Combined.HitRatePercent,
                    ["CurrentKeyCount"] = cacheStats.Combined.CurrentKeyCount,
                    ["TotalBytesStored"] = cacheStats.Combined.TotalBytesStored
                },
                
                // Additional computed metrics
                ["AdditionalMetrics"] = cacheStats.AdditionalMetrics
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
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogWarning("Clearing all cache data");

            var totalCleared = 0;

            // Clear memory cache
            var memoryStopwatch = Stopwatch.StartNew();
            var memoryKeys = _memoryKeyTracker.Keys.ToList();
            foreach (var key in memoryKeys)
            {
                _memoryCache.Remove(key);
                _memoryKeyTracker.TryRemove(key, out _);
            }
            memoryStopwatch.Stop();
            totalCleared += memoryKeys.Count;

            _statisticsTracker.TrackPatternOperation("*", memoryKeys.Count, memoryStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Memory);

            // Clear Redis cache if available
            if (_redisAvailable)
            {
                try
                {
                    var redisStopwatch = Stopwatch.StartNew();
                    // Use Redis service to clear all keys with FlightBoard prefix
                    var redisCleared = await _redisService.RemoveByPatternAsync("FlightBoard:*", cancellationToken);
                    redisStopwatch.Stop();
                    totalCleared += redisCleared;
                    
                    _statisticsTracker.TrackPatternOperation("FlightBoard:*", redisCleared, redisStopwatch.Elapsed.TotalMilliseconds, CacheLayer.Redis);
                    _logger.LogInformation("Cleared {RedisCount} keys from Redis", redisCleared);
                }
                catch (Exception redisEx)
                {
                    _logger.LogWarning(redisEx, "Failed to clear Redis cache");
                }
            }

            stopwatch.Stop();
            _statisticsTracker.TrackPatternOperation("*", totalCleared, stopwatch.Elapsed.TotalMilliseconds, CacheLayer.Combined);

            _logger.LogInformation("Cache clear completed - Total cleared: {TotalCount} (Memory: {MemoryCount}) in {ElapsedMs}ms", 
                totalCleared, memoryKeys.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            throw;
        }
    }
}
