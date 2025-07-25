using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.Contract.Performance;

/// <summary>
/// Public contract for Performance Manager - Performance monitoring orchestration interface
/// Following iDesign Method: Only public manager contracts are in Contract namespace
/// Orchestrates performance monitoring, metrics collection, and cache management
/// </summary>
public interface IPerformanceManager
{
    /// <summary>
    /// Get comprehensive performance summary including metrics and cache stats
    /// </summary>
    Task<PerformanceSummary> GetPerformanceSummaryAsync();
    
    /// <summary>
    /// Track custom performance metric through proper orchestration
    /// </summary>
    Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? properties = null);
    
    /// <summary>
    /// Track custom performance event through proper orchestration
    /// </summary>
    Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null);
    
    /// <summary>
    /// Get cache performance statistics
    /// </summary>
    Task<CacheStatsResponse> GetCacheStatsAsync();
    
    /// <summary>
    /// Clear all cached data (admin operation)
    /// </summary>
    Task ClearAllCacheAsync();
    
    /// <summary>
    /// Clear cache by pattern (admin operation)
    /// </summary>
    Task ClearCacheByPatternAsync(string pattern);
    
    /// <summary>
    /// Get system health status
    /// </summary>
    Task<SystemHealthResponse> GetSystemHealthAsync();
}

/// <summary>
/// Cache statistics response data contract
/// </summary>
public record CacheStatsResponse(
    int TotalKeys,
    double HitRate,
    long TotalHits,
    long TotalMisses,
    string CacheType,
    Dictionary<string, object> AdditionalStats
);

/// <summary>
/// System health response data contract
/// </summary>
public record SystemHealthResponse(
    string Status,
    TimeSpan Uptime,
    double CpuUsage,
    long MemoryUsage,
    Dictionary<string, string> Services,
    DateTime Timestamp
);
