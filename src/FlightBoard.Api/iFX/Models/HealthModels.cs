namespace FlightBoard.Api.iFX.Models;

/// <summary>
/// Comprehensive health check result containing all system health data
/// </summary>
public class ComprehensiveHealthResult
{
    public required string OverallStatus { get; set; }
    public long CheckDurationMs { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
    public required DatabaseHealthResult Database { get; set; }
    public required RedisHealthResult Redis { get; set; }
    public required CacheHealthResult Cache { get; set; }
    public required SystemResourceMetrics SystemResources { get; set; }
    public required HealthSummary Summary { get; set; }
}

/// <summary>
/// Database health check result
/// </summary>
public class DatabaseHealthResult
{
    public required string Status { get; set; }
    public bool Connected { get; set; }
    public long ResponseTimeMs { get; set; }
    public long QueryPerformanceMs { get; set; }
    public string? Provider { get; set; }
    public int FlightCount { get; set; }
    public int UserCount { get; set; }
    public string? ConnectionString { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Redis health check result
/// </summary>
public class RedisHealthResult
{
    public required string Status { get; set; }
    public bool Connected { get; set; }
    public bool Available { get; set; }
    public long ResponseTimeMs { get; set; }
    public string ServerInfo { get; set; } = "N/A";
    public Dictionary<string, object> ConnectionDetails { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Cache health check result with performance metrics
/// </summary>
public class CacheHealthResult
{
    public required string Status { get; set; }
    public long ResponseTimeMs { get; set; }
    public CacheLayerHealth? MemoryCache { get; set; }
    public CacheLayerHealth? RedisCache { get; set; }
    public CacheLayerHealth? Combined { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Health metrics for individual cache layer
/// </summary>
public class CacheLayerHealth
{
    public required string Status { get; set; }
    public double HitRatePercent { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public int KeyCount { get; set; }
    public long TotalBytesStored { get; set; }
}

/// <summary>
/// Comprehensive system resource metrics
/// </summary>
public class SystemResourceMetrics
{
    public required string Status { get; set; }
    public TimeSpan Uptime { get; set; }
    public CpuMetrics? CPU { get; set; }
    public MemoryMetrics? Memory { get; set; }
    public ThreadMetrics? Threads { get; set; }
    public DiskMetrics? Disk { get; set; }
    public NetworkMetrics? Network { get; set; }
    public EnvironmentMetrics? Environment { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// CPU performance metrics
/// </summary>
public class CpuMetrics
{
    public double UsagePercent { get; set; }
    public int ProcessorCount { get; set; }
}

/// <summary>
/// Memory usage metrics
/// </summary>
public class MemoryMetrics
{
    public long WorkingSetBytes { get; set; }
    public double WorkingSetMB { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public double PrivateMemoryMB { get; set; }
    public long GCMemoryBytes { get; set; }
    public double GCMemoryMB { get; set; }
    public int GCGen0Collections { get; set; }
    public int GCGen1Collections { get; set; }
    public int GCGen2Collections { get; set; }
}

/// <summary>
/// Thread pool metrics
/// </summary>
public class ThreadMetrics
{
    public int TotalThreads { get; set; }
    public int ThreadPoolWorkerThreads { get; set; }
    public int ThreadPoolCompletionThreads { get; set; }
}

/// <summary>
/// Disk usage metrics
/// </summary>
public class DiskMetrics
{
    public long TotalBytes { get; set; }
    public double TotalGB { get; set; }
    public long FreeBytes { get; set; }
    public double FreeGB { get; set; }
    public long UsedBytes { get; set; }
    public double UsedGB { get; set; }
    public double UsagePercent { get; set; }
}

/// <summary>
/// Network connectivity metrics
/// </summary>
public class NetworkMetrics
{
    public bool InternetConnectivity { get; set; }
    public long PingLatencyMs { get; set; }
    public string Status { get; set; } = "Unknown";
}

/// <summary>
/// Environment and runtime information
/// </summary>
public class EnvironmentMetrics
{
    public string MachineName { get; set; } = Environment.MachineName;
    public string OSVersion { get; set; } = Environment.OSVersion.ToString();
    public string ProcessorArchitecture { get; set; } = Environment.OSVersion.Platform.ToString();
    public string RuntimeVersion { get; set; } = Environment.Version.ToString();
    public string AspNetCoreEnvironment { get; set; } = "Unknown";
}

/// <summary>
/// Health check summary with issues and recommendations
/// </summary>
public class HealthSummary
{
    public string[] Issues { get; set; } = Array.Empty<string>();
    public string[] Warnings { get; set; } = Array.Empty<string>();
    public string[] Recommendations { get; set; } = Array.Empty<string>();
}
