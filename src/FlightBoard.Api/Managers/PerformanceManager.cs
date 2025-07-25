using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.iFX.Engines;

namespace FlightBoard.Api.Managers;

/// <summary>
/// Performance manager for orchestrating performance monitoring use cases following iDesign Method principles
/// Controls the flow of performance operations by coordinating engines and services
/// Implements the public contract interface for external consumption
/// </summary>
public class PerformanceManager : IPerformanceManager
{
    private readonly IPerformanceService _performanceService;
    private readonly ICacheService _cacheService;
    private readonly IPerformanceEngine _performanceEngine;
    private readonly ILogger<PerformanceManager> _logger;

    public PerformanceManager(
        IPerformanceService performanceService,
        ICacheService cacheService,
        IPerformanceEngine performanceEngine,
        ILogger<PerformanceManager> logger)
    {
        _performanceService = performanceService;
        _cacheService = cacheService;
        _performanceEngine = performanceEngine;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive performance summary including metrics and cache stats
    /// </summary>
    public async Task<PerformanceSummary> GetPerformanceSummaryAsync()
    {
        try
        {
            using var tracker = _performanceService.TrackOperation("GetPerformanceSummary");
            
            _logger.LogInformation("Retrieving comprehensive performance summary");

            // Get performance data through service
            var summary = await _performanceService.GetSummaryAsync();
            
            // Enhance with additional metrics through engine
            var enhancedSummary = _performanceEngine.EnhancePerformanceSummary(summary);
            
            // Track this operation
            _performanceService.TrackEvent("PerformanceSummaryRequested");
            
            return enhancedSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance summary");
            throw;
        }
    }

    /// <summary>
    /// Track custom performance metric through proper orchestration
    /// </summary>
    public async Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        try
        {
            _logger.LogInformation("Tracking performance metric: {MetricName} = {Value}", metricName, value);

            // Validate through engine
            _performanceEngine.ValidateMetric(metricName, value, properties);
            
            // Track through service
            _performanceService.TrackMetric(metricName, value, properties);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking metric: {MetricName}", metricName);
            throw;
        }
    }

    /// <summary>
    /// Track custom performance event through proper orchestration
    /// </summary>
    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        try
        {
            _logger.LogInformation("Tracking performance event: {EventName}", eventName);

            // Validate through engine
            _performanceEngine.ValidateEvent(eventName, properties);
            
            // Track through service
            _performanceService.TrackEvent(eventName, properties);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking event: {EventName}", eventName);
            throw;
        }
    }

    /// <summary>
    /// Get cache performance statistics
    /// </summary>
    public async Task<CacheStatsResponse> GetCacheStatsAsync()
    {
        try
        {
            using var tracker = _performanceService.TrackOperation("GetCacheStats");
            
            _logger.LogInformation("Retrieving cache performance statistics");

            // Get cache stats through service
            var stats = await _cacheService.GetStatsAsync();
            
            // Process through engine for additional insights
            var processedStats = _performanceEngine.ProcessCacheStats(stats);
            
            return processedStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            throw;
        }
    }

    /// <summary>
    /// Clear all cached data (admin operation)
    /// </summary>
    public async Task ClearAllCacheAsync()
    {
        try
        {
            using var tracker = _performanceService.TrackOperation("ClearAllCache");
            
            _logger.LogWarning("Clearing all cached data - Administrative operation");

            // Validate operation through engine
            _performanceEngine.ValidateCacheClearOperation();
            
            // Clear cache through service
            await _cacheService.ClearAllAsync();
            
            // Track administrative action
            _performanceService.TrackEvent("CacheCleared", new Dictionary<string, string>
            {
                ["Operation"] = "ClearAll",
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            throw;
        }
    }

    /// <summary>
    /// Clear cache by pattern (admin operation)
    /// </summary>
    public async Task ClearCacheByPatternAsync(string pattern)
    {
        try
        {
            using var tracker = _performanceService.TrackOperation("ClearCacheByPattern");
            
            _logger.LogWarning("Clearing cache by pattern: {Pattern} - Administrative operation", pattern);

            // Validate pattern through engine
            _performanceEngine.ValidateCachePattern(pattern);
            
            // Clear cache through service
            await _cacheService.RemoveByPatternAsync(pattern);
            
            // Track administrative action
            _performanceService.TrackEvent("CacheCleared", new Dictionary<string, string>
            {
                ["Operation"] = "ClearByPattern",
                ["Pattern"] = pattern,
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache by pattern: {Pattern}", pattern);
            throw;
        }
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    public async Task<SystemHealthResponse> GetSystemHealthAsync()
    {
        try
        {
            using var tracker = _performanceService.TrackOperation("GetSystemHealth");
            
            _logger.LogInformation("Retrieving system health status");

            // Collect health metrics through engine
            var healthData = await _performanceEngine.CollectSystemHealthDataAsync();
            
            // Track health check
            _performanceService.TrackEvent("SystemHealthChecked");
            
            return healthData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system health");
            throw;
        }
    }
}
