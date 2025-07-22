using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Performance monitoring and metrics API endpoint
/// Provides runtime performance insights and cache statistics
/// Requires Admin authorization for sensitive performance data
/// Uses utility services directly - no Manager layer needed for infrastructure concerns
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
        try
        {
            var summary = await _performanceService.GetSummaryAsync();
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
    /// Clear all cached data (admin operation)
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<ActionResult> ClearAllCache()
    {
        try
        {
            await _cacheService.ClearAllAsync();
            _logger.LogWarning("All cache cleared by admin");
            
            return Ok(new { Message = "All cache cleared successfully" });
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
