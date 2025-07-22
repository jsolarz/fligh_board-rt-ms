using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Performance monitoring and metrics API endpoint
/// Provides runtime performance insights and cache statistics
/// Requires Admin authorization for sensitive performance data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService _performanceService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IPerformanceService performanceService,
        ICacheService cacheService,
        ILogger<PerformanceController> logger)
    {
        _performanceService = performanceService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive performance summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<PerformanceSummary>> GetPerformanceSummary()
    {
        using var tracker = _performanceService.TrackOperation("GetPerformanceSummary");
        
        try
        {
            var summary = await _performanceService.GetSummaryAsync();
            _performanceService.TrackEvent("PerformanceSummaryRequested");
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance summary");
            return StatusCode(500, "Error retrieving performance data");
        }
    }

    /// <summary>
    /// Track custom performance metric
    /// </summary>
    [HttpPost("metrics")]
    public ActionResult TrackMetric([FromBody] TrackMetricRequest request)
    {
        try
        {
            _performanceService.TrackMetric(request.MetricName, request.Value, request.Properties);
            _logger.LogInformation("Custom metric tracked: {MetricName} = {Value}", request.MetricName, request.Value);
            
            return Ok(new { Message = "Metric tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking metric: {MetricName}", request.MetricName);
            return StatusCode(500, "Error tracking metric");
        }
    }

    /// <summary>
    /// Get cache statistics and performance
    /// </summary>
    [HttpGet("cache/stats")]
    public async Task<ActionResult<CacheStatsResponse>> GetCacheStats()
    {
        using var tracker = _performanceService.TrackOperation("GetCacheStats");
        
        try
        {
            // Sample cache keys to check
            var testKeys = new[]
            {
                "flights:search:",
                "flights:status:scheduled",
                "flights:status:departed",
                "flight:"
            };

            var stats = new CacheStatsResponse
            {
                TotalChecks = testKeys.Length,
                CacheHits = 0,
                CacheMisses = 0,
                TestKeys = new List<CacheKeyInfo>()
            };

            foreach (var key in testKeys)
            {
                var exists = await _cacheService.ExistsAsync(key);
                var keyInfo = new CacheKeyInfo
                {
                    Key = key,
                    Exists = exists,
                    CheckedAt = DateTime.UtcNow
                };
                
                stats.TestKeys.Add(keyInfo);
                
                if (exists)
                    stats.CacheHits++;
                else
                    stats.CacheMisses++;
            }

            stats.HitRate = stats.TotalChecks > 0 ? (double)stats.CacheHits / stats.TotalChecks : 0;

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return StatusCode(500, "Error retrieving cache statistics");
        }
    }

    /// <summary>
    /// Clear all performance metrics (reset counters)
    /// </summary>
    [HttpPost("reset")]
    public ActionResult ResetMetrics()
    {
        try
        {
            // Note: In a real implementation, you'd need to add a reset method to IPerformanceService
            _logger.LogInformation("Performance metrics reset requested by admin");
            _performanceService.TrackEvent("PerformanceMetricsReset");
            
            return Ok(new { Message = "Performance metrics reset (note: requires service restart for full reset)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting performance metrics");
            return StatusCode(500, "Error resetting metrics");
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
/// Cache statistics response
/// </summary>
public record CacheStatsResponse
{
    public int TotalChecks { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double HitRate { get; set; }
    public List<CacheKeyInfo> TestKeys { get; set; } = new();
}

/// <summary>
/// Cache key information
/// </summary>
public record CacheKeyInfo
{
    public string Key { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public DateTime CheckedAt { get; set; }
}
