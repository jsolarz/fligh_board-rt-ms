using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.iFX.Engines;

/// <summary>
/// Performance engine interface for business logic related to performance monitoring
/// Following iDesign Method: Engine interfaces kept within their respective projects
/// </summary>
public interface IPerformanceEngine
{
    /// <summary>
    /// Enhance performance summary with additional computed metrics
    /// </summary>
    PerformanceSummary EnhancePerformanceSummary(PerformanceSummary baseSummary);
    
    /// <summary>
    /// Validate metric parameters before tracking
    /// </summary>
    void ValidateMetric(string metricName, double value, Dictionary<string, string>? properties);
    
    /// <summary>
    /// Validate event parameters before tracking
    /// </summary>
    void ValidateEvent(string eventName, Dictionary<string, string>? properties);
    
    /// <summary>
    /// Process cache statistics and add computed insights
    /// </summary>
    CacheStatsResponse ProcessCacheStats(Dictionary<string, object> rawStats);
    
    /// <summary>
    /// Validate cache clear operation permissions and safety
    /// </summary>
    void ValidateCacheClearOperation();
    
    /// <summary>
    /// Validate cache pattern for safety and syntax
    /// </summary>
    void ValidateCachePattern(string pattern);
    
    /// <summary>
    /// Collect comprehensive system health data
    /// </summary>
    Task<SystemHealthResponse> CollectSystemHealthDataAsync();
}
