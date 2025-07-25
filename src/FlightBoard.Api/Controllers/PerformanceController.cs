using Microsoft.AspNetCore.Mvc;
using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.Attributes;
using Asp.Versioning;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Performance monitoring and metrics controller
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersioned("1.0")]
[Produces("application/json")]
[Compression(Enabled = true)]
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
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 60)]
    [ResponseCache(CacheProfileName = "Short")]
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

            // Add performance headers
            Response.Headers.Add("X-Analytics-Generated", DateTime.UtcNow.ToString("O"));
            Response.Headers.Add("X-Cache-Efficiency", analytics.AdditionalMetrics.GetValueOrDefault("OverallEfficiency", 0).ToString());

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
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult> GetCacheStats()
    {
        try
        {
            var stats = await _cacheService.GetStatsAsync();
            
            // Add cache performance headers
            Response.Headers.Add("X-Stats-Generated", DateTime.UtcNow.ToString("O"));
            if (stats.TryGetValue("RedisAvailable", out var redisStatus))
            {
                Response.Headers.Add("X-Redis-Status", redisStatus.ToString());
            }
            
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
    [ResponseCache(CacheProfileName = "NoCache")]
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
    [ResponseCache(CacheProfileName = "NoCache")]
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

    /// <summary>
    /// Clear cache by pattern (admin operation)
    /// </summary>
    [HttpPost("cache/clear/pattern")]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> ClearCacheByPattern([FromBody] ClearCachePatternRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Pattern))
                return BadRequest("Pattern is required");

            await _performanceManager.ClearCacheByPatternAsync(request.Pattern);
            _logger.LogWarning("Cache cleared by pattern: {Pattern}", request.Pattern);
            
            return Ok(new 
            { 
                Message = $"Cache cleared for pattern: {request.Pattern}",
                Pattern = request.Pattern,
                ClearedAt = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache by pattern: {Pattern}", request.Pattern);
            return StatusCode(500, "Error clearing cache by pattern");
        }
    }

    /// <summary>
    /// Track custom performance metric
    /// </summary>
    [HttpPost("metrics/track")]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> TrackMetric([FromBody] TrackMetricRequest request)
    {
        try
        {
            await _performanceManager.TrackMetricAsync(request.MetricName, request.Value, request.Properties);
            return Ok(new { Message = "Metric tracked successfully", Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking metric: {MetricName}", request.MetricName);
            return StatusCode(500, "Error tracking metric");
        }
    }

    /// <summary>
    /// Get system health including cache performance
    /// </summary>
    [HttpGet("health")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult> GetSystemHealth()
    {
        try
        {
            var health = await _performanceManager.GetSystemHealthAsync();
            
            // Add health status headers
            Response.Headers.Add("X-Health-Status", health.Status);
            Response.Headers.Add("X-Health-Checked", DateTime.UtcNow.ToString("O"));
            
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system health");
            return StatusCode(500, "Error retrieving system health");
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

/// <summary>
/// Request model for clearing cache by pattern
/// </summary>
public record ClearCachePatternRequest(string Pattern);
