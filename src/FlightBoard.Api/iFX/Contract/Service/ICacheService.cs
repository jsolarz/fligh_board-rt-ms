using System.Text.Json;

namespace FlightBoard.Api.iFX.Contract.Service;

/// <summary>
/// Cache service contract for high-performance caching operations
/// Supports both in-memory and distributed Redis caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key with automatic JSON deserialization
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Set cache value with expiration and automatic JSON serialization
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Remove cached value by key
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove multiple cached values by pattern (Redis only)
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get or set cache value with factory function (cache-aside pattern)
    /// </summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Get cache statistics for performance monitoring
    /// </summary>
    Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear all cached data (admin operation)
    /// </summary>
    Task ClearAllAsync(CancellationToken cancellationToken = default);
}
