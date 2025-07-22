namespace FlightBoard.Api.iFX.Contract;

/// <summary>
/// Caching service contract for performance optimization
/// Following iDesign Method: Infrastructure framework contracts
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached item by key
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached item or null if not found</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a cached item with expiration
    /// </summary>
    /// <typeparam name="T">Type of item to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiry">Cache expiry time</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class;

    /// <summary>
    /// Removes an item from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes multiple items from cache by pattern
    /// </summary>
    /// <param name="pattern">Pattern to match cache keys</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if key exists</returns>
    bool Exists(string key);
}
