using System.Diagnostics;

namespace FlightBoard.Api.iFX.Contract.Service;

/// <summary>
/// Contract for tracking cache performance statistics
/// </summary>
public interface ICacheStatisticsTracker
{
    /// <summary>
    /// Track a cache hit with response time
    /// </summary>
    void TrackHit(string key, double responseTimeMs, CacheLayer layer);
    
    /// <summary>
    /// Track a cache miss with response time
    /// </summary>
    void TrackMiss(string key, double responseTimeMs, CacheLayer layer);
    
    /// <summary>
    /// Track a cache set operation
    /// </summary>
    void TrackSet(string key, double responseTimeMs, CacheLayer layer, int valueSizeBytes = 0);
    
    /// <summary>
    /// Track a cache remove operation
    /// </summary>
    void TrackRemove(string key, double responseTimeMs, CacheLayer layer);
    
    /// <summary>
    /// Track a cache pattern operation
    /// </summary>
    void TrackPatternOperation(string pattern, int keysAffected, double responseTimeMs, CacheLayer layer);
    
    /// <summary>
    /// Get comprehensive cache statistics
    /// </summary>
    CacheStatistics GetStatistics();
    
    /// <summary>
    /// Reset all statistics
    /// </summary>
    void Reset();
}

/// <summary>
/// Cache layer enumeration
/// </summary>
public enum CacheLayer
{
    Memory,
    Redis,
    Combined
}

/// <summary>
/// Comprehensive cache statistics
/// </summary>
public record CacheStatistics(
    DateTime StartTime,
    TimeSpan Uptime,
    CacheLayerStats Memory,
    CacheLayerStats Redis,
    CacheLayerStats Combined,
    Dictionary<string, object> AdditionalMetrics
);

/// <summary>
/// Statistics for a specific cache layer
/// </summary>
public record CacheLayerStats(
    long TotalHits,
    long TotalMisses,
    long TotalSets,
    long TotalRemoves,
    long TotalRequests,
    double HitRatePercent,
    double AverageResponseTimeMs,
    double MinResponseTimeMs,
    double MaxResponseTimeMs,
    int CurrentKeyCount,
    long TotalBytesStored,
    Dictionary<string, CacheOperationStats> OperationStats
);

/// <summary>
/// Statistics for specific cache operations
/// </summary>
public record CacheOperationStats(
    long Count,
    double AverageMs,
    double MinMs,
    double MaxMs,
    long TotalBytes
);
