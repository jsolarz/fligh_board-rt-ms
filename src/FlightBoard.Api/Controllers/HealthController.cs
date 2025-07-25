using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using System.Reflection;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Health check controller for monitoring application status
/// </summary>
[ApiController]
[Route("api/flights/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly FlightDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(FlightDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetHealth()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Database = new
                {
                    Connected = canConnect,
                    Provider = _context.Database.ProviderName
                },
                Uptime = Environment.TickCount64
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            var unhealthyStatus = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            };

            return StatusCode(503, unhealthyStatus);
        }
    }

    /// <summary>
    /// Detailed health check with component status
    /// </summary>
    [HttpGet("detailed")]
    public async Task<ActionResult<object>> GetDetailedHealth()
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
                        Status = "Healthy", // Could add cache-specific health checks
                        Type = "Memory"
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
            _logger.LogError(ex, "Detailed health check failed");
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

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
}

/// <summary>
/// Health check result model for database status
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
