using StackExchange.Redis;

namespace FlightBoard.Api.iFX.Contract.Service;

/// <summary>
/// Contract for Redis-specific operations that require direct database access
/// </summary>
public interface IRedisService
{
    /// <summary>
    /// Remove keys matching a pattern using SCAN (safe for production)
    /// </summary>
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all keys matching a pattern using SCAN
    /// </summary>
    Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if Redis is available
    /// </summary>
    Task<bool> IsAvailableAsync();
    
    /// <summary>
    /// Get Redis database info
    /// </summary>
    Task<Dictionary<string, object>> GetInfoAsync();
}
