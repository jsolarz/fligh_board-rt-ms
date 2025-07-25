using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.Data;
using FlightBoard.Api.iFX.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace FlightBoard.Api.iFX.Engines;

/// <summary>
/// Health check engine for comprehensive system health monitoring
/// </summary>
public interface IHealthCheckEngine
{
    /// <summary>
    /// Perform comprehensive health check of all system components
    /// </summary>
    Task<ComprehensiveHealthResult> PerformHealthCheckAsync();
    
    /// <summary>
    /// Check database connectivity and performance
    /// </summary>
    Task<DatabaseHealthResult> CheckDatabaseHealthAsync();
    
    /// <summary>
    /// Check Redis connectivity and performance
    /// </summary>
    Task<RedisHealthResult> CheckRedisHealthAsync();
    
    /// <summary>
    /// Check cache performance metrics
    /// </summary>
    Task<CacheHealthResult> CheckCacheHealthAsync();
    
    /// <summary>
    /// Get detailed system resource metrics
    /// </summary>
    Task<SystemResourceMetrics> GetSystemResourceMetricsAsync();
}

/// <summary>
/// Health check engine implementation
/// </summary>
public class HealthCheckEngine : IHealthCheckEngine
{
    private readonly FlightDbContext _dbContext;
    private readonly IRedisService _redisService;
    private readonly ICacheService _cacheService;
    private readonly ICacheStatisticsTracker _cacheStatistics;
    private readonly ILogger<HealthCheckEngine> _logger;

    public HealthCheckEngine(
        FlightDbContext dbContext,
        IRedisService redisService,
        ICacheService cacheService,
        ICacheStatisticsTracker cacheStatistics,
        ILogger<HealthCheckEngine> logger)
    {
        _dbContext = dbContext;
        _redisService = redisService;
        _cacheService = cacheService;
        _cacheStatistics = cacheStatistics;
        _logger = logger;
    }

    public async Task<ComprehensiveHealthResult> PerformHealthCheckAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Run health checks in parallel
            var databaseHealthTask = CheckDatabaseHealthAsync();
            var redisHealthTask = CheckRedisHealthAsync();
            var cacheHealthTask = CheckCacheHealthAsync();
            var systemMetricsTask = GetSystemResourceMetricsAsync();

            await Task.WhenAll(databaseHealthTask, redisHealthTask, cacheHealthTask, systemMetricsTask);

            var databaseHealth = await databaseHealthTask;
            var redisHealth = await redisHealthTask;
            var cacheHealth = await cacheHealthTask;
            var systemMetrics = await systemMetricsTask;

            stopwatch.Stop();

            // Determine overall health status
            var overallStatus = DetermineOverallStatus(databaseHealth!, redisHealth!, cacheHealth!, systemMetrics!);

            return new ComprehensiveHealthResult
            {
                OverallStatus = overallStatus,
                CheckDurationMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                Database = databaseHealth!,
                Redis = redisHealth!,
                Cache = cacheHealth!,
                SystemResources = systemMetrics!,
                Summary = GenerateHealthSummary(databaseHealth!, redisHealth!, cacheHealth!, systemMetrics!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Comprehensive health check failed");
            stopwatch.Stop();
            
            return new ComprehensiveHealthResult
            {
                OverallStatus = "Critical",
                CheckDurationMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                Error = ex.Message,
                Database = new DatabaseHealthResult { Status = "Error", Error = "Health check failed" },
                Redis = new RedisHealthResult { Status = "Error", Error = "Health check failed" },
                Cache = new CacheHealthResult { Status = "Error", Error = "Health check failed" },
                SystemResources = new SystemResourceMetrics { Status = "Error" },
                Summary = new HealthSummary { Issues = new[] { "Health check system failure" } }
            };
        }
    }

    public async Task<DatabaseHealthResult> CheckDatabaseHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Test basic connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                return new DatabaseHealthResult
                {
                    Status = "Unhealthy",
                    Connected = false,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Error = "Cannot connect to database"
                };
            }

            // Test query performance
            var queryStopwatch = Stopwatch.StartNew();
            var flightCount = await _dbContext.Flights.CountAsync();
            var userCount = await _dbContext.Users.CountAsync();
            queryStopwatch.Stop();

            stopwatch.Stop();

            return new DatabaseHealthResult
            {
                Status = queryStopwatch.ElapsedMilliseconds < 1000 ? "Healthy" : "Degraded",
                Connected = true,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                QueryPerformanceMs = queryStopwatch.ElapsedMilliseconds,
                Provider = _dbContext.Database.ProviderName ?? "Unknown",
                FlightCount = flightCount,
                UserCount = userCount,
                ConnectionString = _dbContext.Database.GetConnectionString()?[..50] + "..." // Truncated for security
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DatabaseHealthResult
            {
                Status = "Unhealthy",
                Connected = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    public async Task<RedisHealthResult> CheckRedisHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var isAvailable = await _redisService.IsAvailableAsync();
            var redisInfo = await _redisService.GetInfoAsync();
            
            stopwatch.Stop();

            return new RedisHealthResult
            {
                Status = isAvailable ? "Healthy" : "Degraded",
                Connected = redisInfo.GetValueOrDefault("Connected", false).ToString() == "True",
                Available = isAvailable,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ServerInfo = redisInfo.GetValueOrDefault("ServerInfo", "N/A").ToString() ?? "N/A",
                ConnectionDetails = redisInfo
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new RedisHealthResult
            {
                Status = "Unhealthy",
                Connected = false,
                Available = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    public async Task<CacheHealthResult> CheckCacheHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Get cache statistics
            var stats = _cacheStatistics.GetStatistics();
            var cacheStats = await _cacheService.GetStatsAsync();
            
            stopwatch.Stop();

            // Determine cache health based on performance metrics
            var memoryHitRate = stats.Memory.HitRatePercent;
            var redisHitRate = stats.Redis.HitRatePercent;
            var combinedHitRate = stats.Combined.HitRatePercent;
            
            var status = "Healthy";
            if (combinedHitRate < 50) status = "Degraded";
            if (combinedHitRate < 25) status = "Unhealthy";

            return new CacheHealthResult
            {
                Status = status,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                MemoryCache = new CacheLayerHealth
                {
                    Status = memoryHitRate > 70 ? "Healthy" : "Degraded",
                    HitRatePercent = memoryHitRate,
                    AverageResponseTimeMs = stats.Memory.AverageResponseTimeMs,
                    KeyCount = stats.Memory.CurrentKeyCount,
                    TotalBytesStored = stats.Memory.TotalBytesStored
                },
                RedisCache = new CacheLayerHealth
                {
                    Status = redisHitRate > 60 ? "Healthy" : "Degraded",
                    HitRatePercent = redisHitRate,
                    AverageResponseTimeMs = stats.Redis.AverageResponseTimeMs,
                    KeyCount = 0, // Redis key count would need separate tracking
                    TotalBytesStored = stats.Redis.TotalBytesStored
                },
                Combined = new CacheLayerHealth
                {
                    Status = status,
                    HitRatePercent = combinedHitRate,
                    AverageResponseTimeMs = (stats.Memory.AverageResponseTimeMs + stats.Redis.AverageResponseTimeMs) / 2,
                    KeyCount = stats.Combined.CurrentKeyCount,
                    TotalBytesStored = stats.Combined.TotalBytesStored
                },
                PerformanceMetrics = stats.AdditionalMetrics
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new CacheHealthResult
            {
                Status = "Error",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    public async Task<SystemResourceMetrics> GetSystemResourceMetricsAsync()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var startTime = process.StartTime;
            var uptime = DateTime.UtcNow - startTime;

            // CPU usage calculation
            var cpuUsage = await GetCpuUsageAsync();

            // Memory metrics
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var gcMemory = GC.GetTotalMemory(false);
            var gcGen0 = GC.CollectionCount(0);
            var gcGen1 = GC.CollectionCount(1);
            var gcGen2 = GC.CollectionCount(2);

            // Thread metrics
            var threadCount = process.Threads.Count;
            var threadPoolWorker = 0;
            var threadPoolCompletion = 0;
            ThreadPool.GetAvailableThreads(out threadPoolWorker, out threadPoolCompletion);

            // Disk usage (basic)
            var diskInfo = GetDiskUsage();

            // Network connectivity
            var networkStatus = await CheckNetworkConnectivity();

            var status = DetermineSystemResourceStatus(cpuUsage, workingSet, diskInfo.UsagePercent);

            return new SystemResourceMetrics
            {
                Status = status,
                Uptime = uptime,
                CPU = new CpuMetrics
                {
                    UsagePercent = cpuUsage,
                    ProcessorCount = Environment.ProcessorCount
                },
                Memory = new MemoryMetrics
                {
                    WorkingSetBytes = workingSet,
                    WorkingSetMB = Math.Round(workingSet / 1024.0 / 1024.0, 2),
                    PrivateMemoryBytes = privateMemory,
                    PrivateMemoryMB = Math.Round(privateMemory / 1024.0 / 1024.0, 2),
                    GCMemoryBytes = gcMemory,
                    GCMemoryMB = Math.Round(gcMemory / 1024.0 / 1024.0, 2),
                    GCGen0Collections = gcGen0,
                    GCGen1Collections = gcGen1,
                    GCGen2Collections = gcGen2
                },
                Threads = new ThreadMetrics
                {
                    TotalThreads = threadCount,
                    ThreadPoolWorkerThreads = threadPoolWorker,
                    ThreadPoolCompletionThreads = threadPoolCompletion
                },
                Disk = diskInfo,
                Network = networkStatus,
                Environment = new EnvironmentMetrics
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorArchitecture = Environment.OSVersion.Platform.ToString(),
                    RuntimeVersion = Environment.Version.ToString(),
                    AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                }
            };
        }
        catch (Exception ex)
        {
            return new SystemResourceMetrics
            {
                Status = "Error",
                Error = ex.Message
            };
        }
    }

    #region Private Helper Methods

    private static async Task<double> GetCpuUsageAsync()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        
        await Task.Delay(500);
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return Math.Round(cpuUsageTotal * 100, 2);
    }

    private static DiskMetrics GetDiskUsage()
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory)!);
            var totalBytes = drive.TotalSize;
            var freeBytes = drive.TotalFreeSpace;
            var usedBytes = totalBytes - freeBytes;
            var usagePercent = Math.Round((double)usedBytes / totalBytes * 100, 2);

            return new DiskMetrics
            {
                TotalBytes = totalBytes,
                TotalGB = Math.Round(totalBytes / 1024.0 / 1024.0 / 1024.0, 2),
                FreeBytes = freeBytes,
                FreeGB = Math.Round(freeBytes / 1024.0 / 1024.0 / 1024.0, 2),
                UsedBytes = usedBytes,
                UsedGB = Math.Round(usedBytes / 1024.0 / 1024.0 / 1024.0, 2),
                UsagePercent = usagePercent
            };
        }
        catch
        {
            return new DiskMetrics { UsagePercent = 0, TotalGB = 0, FreeGB = 0, UsedGB = 0 };
        }
    }

    private static async Task<NetworkMetrics> CheckNetworkConnectivity()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            
            return new NetworkMetrics
            {
                InternetConnectivity = reply.Status == IPStatus.Success,
                PingLatencyMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : 0,
                Status = reply.Status.ToString()
            };
        }
        catch
        {
            return new NetworkMetrics
            {
                InternetConnectivity = false,
                PingLatencyMs = 0,
                Status = "Error"
            };
        }
    }

    private static string DetermineOverallStatus(DatabaseHealthResult db, RedisHealthResult redis, CacheHealthResult cache, SystemResourceMetrics system)
    {
        var statuses = new[] { db.Status, cache.Status, system.Status };
        
        if (statuses.Any(s => s == "Critical" || s == "Error")) return "Critical";
        if (statuses.Any(s => s == "Unhealthy")) return "Unhealthy";
        if (statuses.Any(s => s == "Degraded")) return "Degraded";
        return "Healthy";
    }

    private static string DetermineSystemResourceStatus(double cpuUsage, long memoryUsage, double diskUsage)
    {
        if (cpuUsage > 90 || memoryUsage > 2_000_000_000 || diskUsage > 95) return "Critical"; // 2GB RAM, 95% disk
        if (cpuUsage > 75 || memoryUsage > 1_500_000_000 || diskUsage > 85) return "Degraded"; // 1.5GB RAM, 85% disk
        return "Healthy";
    }

    private static HealthSummary GenerateHealthSummary(DatabaseHealthResult db, RedisHealthResult redis, CacheHealthResult cache, SystemResourceMetrics system)
    {
        var issues = new List<string>();
        var warnings = new List<string>();
        var recommendations = new List<string>();

        // Database issues
        if (db.Status == "Unhealthy") issues.Add($"Database connectivity issues: {db.Error}");
        if (db.QueryPerformanceMs > 1000) warnings.Add($"Slow database queries: {db.QueryPerformanceMs}ms");

        // Redis issues
        if (redis.Status == "Unhealthy") issues.Add($"Redis connectivity issues: {redis.Error}");
        if (!redis.Connected) warnings.Add("Redis not connected - using memory cache only");

        // Cache issues
        if (cache.Combined?.HitRatePercent < 50) warnings.Add($"Low cache hit rate: {cache.Combined.HitRatePercent:F1}%");
        if (cache.Combined?.HitRatePercent < 70) recommendations.Add("Consider cache TTL optimization");

        // System resource issues
        if (system.CPU?.UsagePercent > 75) warnings.Add($"High CPU usage: {system.CPU.UsagePercent:F1}%");
        if (system.Memory?.WorkingSetMB > 1500) warnings.Add($"High memory usage: {system.Memory.WorkingSetMB:F1}MB");
        if (system.Disk?.UsagePercent > 85) warnings.Add($"High disk usage: {system.Disk.UsagePercent:F1}%");

        return new HealthSummary
        {
            Issues = issues.ToArray(),
            Warnings = warnings.ToArray(),
            Recommendations = recommendations.ToArray()
        };
    }

    #endregion
}
