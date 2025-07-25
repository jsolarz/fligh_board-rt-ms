using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.iFX.Engines;
using FlightBoard.Api.Models;
using FlightBoard.Api.Attributes;
using Asp.Versioning;
using System.Reflection;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Health check controller with comprehensive system monitoring
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("health")] // Also support legacy route
[ApiVersion("1.0")]
[Produces("application/json")]
[Compression(Enabled = true)]
public class HealthController : ControllerBase
{
    private readonly FlightDbContext _context;
    private readonly IHealthCheckEngine _healthCheckEngine;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        FlightDbContext context,
        IHealthCheckEngine healthCheckEngine,
        ILogger<HealthController> logger)
    {
        _context = context;
        _healthCheckEngine = healthCheckEngine;
        _logger = logger;
    }

    /// <summary>
    /// Simple health check endpoint for load balancers
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
    public ActionResult GetHealth()
    {
        try
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Basic health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Detailed health check with all system components
    /// </summary>
    [HttpGet("detailed")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult<ComprehensiveHealthResult>> GetDetailedHealth()
    {
        try
        {
            var healthResult = await _healthCheckEngine.PerformHealthCheckAsync();
            
            // Add response headers for monitoring tools
            Response.Headers["X-Health-Status"] = healthResult.OverallStatus;
            Response.Headers["X-Health-Check-Duration"] = healthResult.CheckDurationMs.ToString();
            Response.Headers["X-Health-Timestamp"] = healthResult.Timestamp.ToString("O");

            // Return appropriate HTTP status based on health
            var statusCode = healthResult.OverallStatus switch
            {
                "Healthy" => 200,
                "Degraded" => 200, // Still serving traffic
                "Unhealthy" => 503,
                "Critical" => 503,
                "Error" => 500,
                _ => 503
            };

            return StatusCode(statusCode, healthResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            
            return StatusCode(500, new ComprehensiveHealthResult
            {
                OverallStatus = "Error",
                CheckDurationMs = 0,
                Timestamp = DateTime.UtcNow,
                Error = ex.Message,
                Database = new DatabaseHealthResult { Status = "Error", Error = "Health check failed" },
                Redis = new RedisHealthResult { Status = "Error", Error = "Health check failed" },
                Cache = new CacheHealthResult { Status = "Error", Error = "Health check failed" },
                SystemResources = new SystemResourceMetrics { Status = "Error", Error = "Health check failed" },
                Summary = new HealthSummary { Issues = new[] { "Health check system failure" } }
            });
        }
    }

    /// <summary>
    /// Database-specific health check
    /// </summary>
    [HttpGet("database")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 60)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult<DatabaseHealthResult>> GetDatabaseHealth()
    {
        try
        {
            var dbHealth = await _healthCheckEngine.CheckDatabaseHealthAsync();
            
            Response.Headers["X-DB-Status"] = dbHealth.Status;
            Response.Headers["X-DB-Response-Time"] = dbHealth.ResponseTimeMs.ToString();

            var statusCode = dbHealth.Status switch
            {
                "Healthy" => 200,
                "Degraded" => 200,
                "Unhealthy" => 503,
                _ => 500
            };

            return StatusCode(statusCode, dbHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(500, new DatabaseHealthResult
            {
                Status = "Error",
                Connected = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Redis-specific health check
    /// </summary>
    [HttpGet("redis")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult<RedisHealthResult>> GetRedisHealth()
    {
        try
        {
            var redisHealth = await _healthCheckEngine.CheckRedisHealthAsync();
            
            Response.Headers["X-Redis-Status"] = redisHealth.Status;
            Response.Headers["X-Redis-Connected"] = redisHealth.Connected.ToString();

            return Ok(redisHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return StatusCode(500, new RedisHealthResult
            {
                Status = "Error",
                Connected = false,
                Available = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Cache performance health check
    /// </summary>
    [HttpGet("cache")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult<CacheHealthResult>> GetCacheHealth()
    {
        try
        {
            var cacheHealth = await _healthCheckEngine.CheckCacheHealthAsync();
            
            Response.Headers["X-Cache-Status"] = cacheHealth.Status;
            Response.Headers["X-Cache-Hit-Rate"] = cacheHealth.Combined?.HitRatePercent.ToString("F1") ?? "0";

            return Ok(cacheHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache health check failed");
            return StatusCode(500, new CacheHealthResult
            {
                Status = "Error",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// System resources health check
    /// </summary>
    [HttpGet("system")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 30)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult<SystemResourceMetrics>> GetSystemHealth()
    {
        try
        {
            var systemHealth = await _healthCheckEngine.GetSystemResourceMetricsAsync();
            
            Response.Headers["X-System-Status"] = systemHealth.Status;
            Response.Headers["X-CPU-Usage"] = systemHealth.CPU?.UsagePercent.ToString("F1") ?? "0";
            Response.Headers["X-Memory-Usage-MB"] = systemHealth.Memory?.WorkingSetMB.ToString("F1") ?? "0";

            return Ok(systemHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System health check failed");
            return StatusCode(500, new SystemResourceMetrics
            {
                Status = "Error",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Legacy detailed health check (maintains backward compatibility)
    /// </summary>
    [HttpGet("details")]
    [HighPerformance(EnableCaching = true, CacheDurationSeconds = 60)]
    [ResponseCache(CacheProfileName = "Short")]
    public async Task<ActionResult> GetLegacyDetailedHealth()
    {
        try
        {
            // Database health
            var dbHealth = await CheckDatabaseHealth();
            
            // Memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            
            var health = new
            {
                Status = dbHealth.IsHealthy ? "Healthy" : "Degraded",
                Timestamp = DateTime.UtcNow,
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Components = new
                {
                    Database = dbHealth,
                    Memory = new
                    {
                        Status = memoryUsage < 500_000_000 ? "Healthy" : "Warning", // 500MB threshold
                        UsageBytes = memoryUsage,
                        UsageMB = Math.Round(memoryUsage / 1024.0 / 1024.0, 2)
                    },
                    Cache = new
                    {
                        Status = "Healthy", // Basic status
                        Type = "Hybrid"
                    }
                },
                Uptime = new
                {
                    Milliseconds = Environment.TickCount64,
                    Formatted = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss")
                }
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Legacy detailed health check failed");
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Readiness probe for Kubernetes
    /// </summary>
    [HttpGet("ready")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult> GetReadiness()
    {
        try
        {
            // Check critical dependencies
            var canConnectToDb = await _context.Database.CanConnectAsync();
            
            if (!canConnectToDb)
            {
                return StatusCode(503, new { Status = "NotReady", Reason = "Database not available" });
            }

            return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { Status = "NotReady", Error = ex.Message });
        }
    }

    /// <summary>
    /// Liveness probe for Kubernetes
    /// </summary>
    [HttpGet("live")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
    public ActionResult GetLiveness()
    {
        // Simple liveness check - if we can respond, we're alive
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    #region Private Helper Methods

    private async Task<HealthCheckResult> CheckDatabaseHealth()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return new HealthCheckResult
                {
                    Status = "Unhealthy",
                    IsHealthy = false,
                    Connected = false,
                    Error = "Cannot connect to database"
                };
            }

            // Check if we can query data
            var flightCount = await _context.Flights.CountAsync();
            var userCount = await _context.Users.CountAsync();

            return new HealthCheckResult
            {
                Status = "Healthy",
                IsHealthy = true,
                Connected = true,
                Provider = _context.Database.ProviderName,
                FlightCount = flightCount,
                UserCount = userCount
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                Status = "Unhealthy",
                IsHealthy = false,
                Connected = false,
                Error = ex.Message
            };
        }
    }

    #endregion
}

/// <summary>
/// Legacy health check result model for backward compatibility
/// </summary>
public class HealthCheckResult
{
    public required string Status { get; set; }
    public required bool IsHealthy { get; set; }
    public required bool Connected { get; set; }
    public string? Provider { get; set; }
    public int? FlightCount { get; set; }
    public int? UserCount { get; set; }
    public string? Error { get; set; }
}
