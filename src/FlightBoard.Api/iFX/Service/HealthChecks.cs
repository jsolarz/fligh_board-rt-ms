using Microsoft.Extensions.Diagnostics.HealthChecks;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Redis connectivity health check
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IRedisService _redisService;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IRedisService redisService, ILogger<RedisHealthCheck> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await _redisService.IsAvailableAsync();
            var redisInfo = await _redisService.GetInfoAsync();

            if (isAvailable)
            {
                return HealthCheckResult.Healthy("Redis is available and responding", data: redisInfo);
            }

            return HealthCheckResult.Degraded("Redis is not available - using memory cache fallback", data: redisInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis health check failed", exception: ex);
        }
    }
}

/// <summary>
/// Cache performance health check
/// </summary>
public class CachePerformanceHealthCheck : IHealthCheck
{
    private readonly ICacheStatisticsTracker _cacheStatistics;
    private readonly ILogger<CachePerformanceHealthCheck> _logger;

    public CachePerformanceHealthCheck(ICacheStatisticsTracker cacheStatistics, ILogger<CachePerformanceHealthCheck> logger)
    {
        _cacheStatistics = cacheStatistics;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = _cacheStatistics.GetStatistics();
            
            var data = new Dictionary<string, object>
            {
                ["CombinedHitRate"] = stats.Combined.HitRatePercent,
                ["MemoryHitRate"] = stats.Memory.HitRatePercent,
                ["RedisHitRate"] = stats.Redis.HitRatePercent,
                ["MemoryResponseTime"] = stats.Memory.AverageResponseTimeMs,
                ["RedisResponseTime"] = stats.Redis.AverageResponseTimeMs,
                ["TotalKeys"] = stats.Combined.CurrentKeyCount,
                ["MemoryKeys"] = stats.Memory.CurrentKeyCount,
                ["OverallEfficiency"] = stats.AdditionalMetrics.GetValueOrDefault("OverallEfficiency", 0)
            };

            // Determine health based on performance metrics
            var combinedHitRate = stats.Combined.HitRatePercent;
            var memoryResponseTime = stats.Memory.AverageResponseTimeMs;
            var redisResponseTime = stats.Redis.AverageResponseTimeMs;

            if (combinedHitRate >= 70 && memoryResponseTime <= 10 && redisResponseTime <= 100)
            {
                return HealthCheckResult.Healthy("Cache performance is optimal", data: data);
            }
            else if (combinedHitRate >= 50 && memoryResponseTime <= 20 && redisResponseTime <= 200)
            {
                return HealthCheckResult.Degraded("Cache performance is acceptable but could be improved", data: data);
            }
            else
            {
                return HealthCheckResult.Unhealthy("Cache performance is poor", data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache performance health check failed");
            return HealthCheckResult.Unhealthy("Cache performance health check failed", exception: ex);
        }
    }
}

/// <summary>
/// System resources health check
/// </summary>
public class SystemResourcesHealthCheck : IHealthCheck
{
    private readonly ILogger<SystemResourcesHealthCheck> _logger;

    public SystemResourcesHealthCheck(ILogger<SystemResourcesHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            // CPU usage
            var cpuUsage = await GetCpuUsageAsync();
            
            // Memory usage
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            
            // Disk usage
            var diskUsage = GetDiskUsage();

            var data = new Dictionary<string, object>
            {
                ["CpuUsagePercent"] = cpuUsage,
                ["WorkingSetMB"] = Math.Round(workingSet / 1024.0 / 1024.0, 2),
                ["PrivateMemoryMB"] = Math.Round(privateMemory / 1024.0 / 1024.0, 2),
                ["DiskUsagePercent"] = diskUsage,
                ["ProcessorCount"] = Environment.ProcessorCount,
                ["ThreadCount"] = process.Threads.Count
            };

            // Determine health status
            var issues = new List<string>();
            var status = HealthStatus.Healthy;

            if (cpuUsage > 90)
            {
                issues.Add($"High CPU usage: {cpuUsage:F1}%");
                status = HealthStatus.Unhealthy;
            }
            else if (cpuUsage > 75)
            {
                issues.Add($"Elevated CPU usage: {cpuUsage:F1}%");
                status = HealthStatus.Degraded;
            }

            if (workingSet > 2_000_000_000) // 2GB
            {
                issues.Add($"High memory usage: {workingSet / 1024 / 1024:F0}MB");
                status = HealthStatus.Unhealthy;
            }
            else if (workingSet > 1_500_000_000) // 1.5GB
            {
                issues.Add($"Elevated memory usage: {workingSet / 1024 / 1024:F0}MB");
                if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
            }

            if (diskUsage > 95)
            {
                issues.Add($"Critical disk usage: {diskUsage:F1}%");
                status = HealthStatus.Unhealthy;
            }
            else if (diskUsage > 85)
            {
                issues.Add($"High disk usage: {diskUsage:F1}%");
                if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
            }

            var message = issues.Any() 
                ? $"System resources: {string.Join(", ", issues)}"
                : "System resources are healthy";

            return new HealthCheckResult(status, message, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
            return HealthCheckResult.Unhealthy("System resources health check failed", exception: ex);
        }
    }

    private static async Task<double> GetCpuUsageAsync()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
        
        await Task.Delay(500);
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return Math.Round(cpuUsageTotal * 100, 2);
    }

    private static double GetDiskUsage()
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory)!);
            var totalBytes = drive.TotalSize;
            var freeBytes = drive.TotalFreeSpace;
            var usedBytes = totalBytes - freeBytes;
            return Math.Round((double)usedBytes / totalBytes * 100, 2);
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// Memory and garbage collection health check
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var gcMemory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            var data = new Dictionary<string, object>
            {
                ["GCMemoryMB"] = Math.Round(gcMemory / 1024.0 / 1024.0, 2),
                ["Gen0Collections"] = gen0Collections,
                ["Gen1Collections"] = gen1Collections,
                ["Gen2Collections"] = gen2Collections,
                ["TotalCollections"] = gen0Collections + gen1Collections + gen2Collections
            };

            // Simple memory health assessment
            var memoryMB = gcMemory / 1024.0 / 1024.0;
            
            if (memoryMB > 1000) // 1GB
            {
                return Task.FromResult(HealthCheckResult.Degraded("High GC memory usage", data: data));
            }
            else if (gen2Collections > 100) // Many Gen2 collections might indicate memory pressure
            {
                return Task.FromResult(HealthCheckResult.Degraded("High number of Gen2 GC collections", data: data));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Healthy("Memory usage is normal", data: data));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Memory health check failed", exception: ex));
        }
    }
}

/// <summary>
/// Database connectivity health check
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FlightDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(FlightDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Test query performance
            var flightCount = await _dbContext.Flights.CountAsync(cancellationToken);
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["Provider"] = _dbContext.Database.ProviderName ?? "Unknown",
                ["FlightCount"] = flightCount,
                ["UserCount"] = userCount
            };

            return HealthCheckResult.Healthy("Database is healthy and responding", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

/// <summary>
/// Health check response writer for consistent JSON formatting
/// </summary>
public static class HealthCheckResponseWriter
{
    public static async Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = healthReport.Status.ToString(),
            totalDuration = healthReport.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                exception = entry.Value.Exception?.Message,
                tags = entry.Value.Tags
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
}
