using Microsoft.AspNetCore.Mvc;
using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Performance monitoring and metrics controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceManager _performanceManager;
    private readonly ICacheService _cacheService;
    private readonly ICacheStatisticsTracker _cacheStatisticsTracker;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IPerformanceManager performanceManager,
        ICacheService cacheService,
        ICacheStatisticsTracker cacheStatisticsTracker,
        ILogger<PerformanceController> logger)
    {
        _performanceManager = performanceManager;
        _cacheService = cacheService;
        _cacheStatisticsTracker = cacheStatisticsTracker;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive cache performance analytics
    /// </summary>
    [HttpGet("cache/analytics")]
    public ActionResult GetCacheAnalytics()
    {
        try
        {
            var analytics = _cacheStatisticsTracker.GetStatistics();
            
            var response = new
            {
                StartTime = analytics.StartTime,
                Uptime = new
                {
                    TotalHours = Math.Round(analytics.Uptime.TotalHours, 2),
                    Formatted = analytics.Uptime.ToString(@"dd\.hh\:mm\:ss")
                },
                Memory = new
                {
                    analytics.Memory.TotalHits,
                    analytics.Memory.TotalMisses,
                    analytics.Memory.HitRatePercent,
                    analytics.Memory.AverageResponseTimeMs,
                    analytics.Memory.CurrentKeyCount,
                    analytics.Memory.TotalBytesStored
                },
                Redis = new
                {
                    analytics.Redis.TotalHits,
                    analytics.Redis.TotalMisses,
                    analytics.Redis.HitRatePercent,
                    analytics.Redis.AverageResponseTimeMs,
                    analytics.Redis.TotalBytesStored
                },
                Combined = new
                {
                    analytics.Combined.TotalHits,
                    analytics.Combined.TotalMisses,
                    analytics.Combined.HitRatePercent,
                    analytics.Combined.TotalBytesStored
                },
                AdditionalMetrics = analytics.AdditionalMetrics
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache analytics");
            return StatusCode(500, "Error retrieving cache analytics");
        }
    }

    /// <summary>
    /// Get basic cache statistics
    /// </summary>
    [HttpGet("cache/stats")]
    public async Task<ActionResult> GetCacheStats()
    {
        try
        {
            var stats = await _cacheService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return StatusCode(500, "Error retrieving cache statistics");
        }
    }

    /// <summary>
    /// Reset cache statistics (admin operation)
    /// </summary>
    [HttpPost("cache/stats/reset")]
    public ActionResult ResetCacheStats()
    {
        try
        {
            _cacheStatisticsTracker.Reset();
            _logger.LogWarning("Cache statistics reset by admin");
            
            return Ok(new { Message = "Cache statistics reset successfully", ResetTime = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting cache statistics");
            return StatusCode(500, "Error resetting cache statistics");
        }
    }

    /// <summary>
    /// Clear all cached data (admin operation)  
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<ActionResult> ClearAllCache()
    {
        try
        {
            await _performanceManager.ClearAllCacheAsync();
            _logger.LogWarning("All cache cleared by admin");
            
            return Ok(new { Message = "All cache cleared successfully", ClearedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            return StatusCode(500, "Error clearing cache");
        }
    }
}

/// <summary>
/// Request model for tracking custom metrics
/// </summary>
public record TrackMetricRequest(
    string MetricName,
    double Value,
    Dictionary<string, string>? Properties = null
);
