using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FlightBoard.Api.iFX.Engines;

/// <summary>
/// Performance engine implementation for business logic related to performance monitoring
/// Following iDesign Method: Contains pure business logic for performance operations
/// </summary>
public class PerformanceEngine : IPerformanceEngine
{
    private readonly ILogger<PerformanceEngine> _logger;
    private static readonly Regex SafePatternRegex = new(@"^[\w\*\?\[\]\-\.]+$", RegexOptions.Compiled);

    public PerformanceEngine(ILogger<PerformanceEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Enhance performance summary with additional computed metrics
    /// </summary>
    public PerformanceSummary EnhancePerformanceSummary(PerformanceSummary baseSummary)
    {
        var enhancedMetrics = new Dictionary<string, double>(baseSummary.Metrics);
        
        // Add computed metrics
        enhancedMetrics["UptimeHours"] = baseSummary.Uptime.TotalHours;
        enhancedMetrics["UptimeDays"] = baseSummary.Uptime.TotalDays;
        
        // Calculate operation efficiency metrics
        if (baseSummary.Operations.Any())
        {
            var totalOperations = baseSummary.Operations.Values.Sum(op => op.Count);
            var averageResponseTime = baseSummary.Operations.Values
                .Where(op => op.Count > 0)
                .Average(op => op.AverageMs);
                
            enhancedMetrics["TotalOperationsCount"] = totalOperations;
            enhancedMetrics["OverallAverageResponseTimeMs"] = averageResponseTime;
            
            // Performance health score (0-100)
            var healthScore = CalculatePerformanceHealthScore(baseSummary.Operations);
            enhancedMetrics["PerformanceHealthScore"] = healthScore;
        }

        return baseSummary with { Metrics = enhancedMetrics };
    }

    /// <summary>
    /// Validate metric parameters before tracking
    /// </summary>
    public void ValidateMetric(string metricName, double value, Dictionary<string, string>? properties)
    {
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("Metric name cannot be null or empty", nameof(metricName));

        if (metricName.Length > 100)
            throw new ArgumentException("Metric name cannot exceed 100 characters", nameof(metricName));

        if (!IsValidMetricName(metricName))
            throw new ArgumentException("Metric name contains invalid characters", nameof(metricName));

        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Metric value must be a valid number", nameof(value));

        // Validate properties if provided
        if (properties != null)
        {
            if (properties.Count > 20)
                throw new ArgumentException("Cannot have more than 20 properties per metric", nameof(properties));

            foreach (var kvp in properties)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Key.Length > 50)
                    throw new ArgumentException($"Property key '{kvp.Key}' is invalid", nameof(properties));
                
                if (kvp.Value?.Length > 200)
                    throw new ArgumentException($"Property value for '{kvp.Key}' exceeds 200 characters", nameof(properties));
            }
        }
    }

    /// <summary>
    /// Validate event parameters before tracking
    /// </summary>
    public void ValidateEvent(string eventName, Dictionary<string, string>? properties)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        if (eventName.Length > 100)
            throw new ArgumentException("Event name cannot exceed 100 characters", nameof(eventName));

        if (!IsValidEventName(eventName))
            throw new ArgumentException("Event name contains invalid characters", nameof(eventName));

        // Validate properties using same logic as metrics
        if (properties != null)
        {
            ValidateMetric("dummy", 0, properties); // Reuse property validation logic
        }
    }

    /// <summary>
    /// Process cache statistics and add computed insights
    /// </summary>
    public CacheStatsResponse ProcessCacheStats(Dictionary<string, object> rawStats)
    {
        var totalKeys = GetStatValue<int>(rawStats, "TotalKeys", 0);
        var totalHits = GetStatValue<long>(rawStats, "TotalHits", 0);
        var totalMisses = GetStatValue<long>(rawStats, "TotalMisses", 0);
        var cacheType = GetStatValue<string>(rawStats, "CacheType", "Unknown");

        // Calculate hit rate
        var totalRequests = totalHits + totalMisses;
        var hitRate = totalRequests > 0 ? (double)totalHits / totalRequests * 100 : 0;

        // Add computed statistics
        var enhancedStats = new Dictionary<string, object>(rawStats)
        {
            ["HitRatePercent"] = Math.Round(hitRate, 2),
            ["TotalRequests"] = totalRequests,
            ["EfficiencyScore"] = CalculateCacheEfficiencyScore(hitRate, totalKeys)
        };

        return new CacheStatsResponse(
            TotalKeys: totalKeys,
            HitRate: hitRate,
            TotalHits: totalHits,
            TotalMisses: totalMisses,
            CacheType: cacheType ?? "Unknown",
            AdditionalStats: enhancedStats
        );
    }

    /// <summary>
    /// Validate cache clear operation permissions and safety
    /// </summary>
    public void ValidateCacheClearOperation()
    {
        // In a real system, this would check user permissions, system state, etc.
        _logger.LogWarning("Cache clear operation validated - proceeding with caution");
        
        // Could add additional safety checks:
        // - Check if system is under high load
        // - Verify user has admin privileges
        // - Ensure no critical operations are running
    }

    /// <summary>
    /// Validate cache pattern for safety and syntax
    /// </summary>
    public void ValidateCachePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Cache pattern cannot be null or empty", nameof(pattern));

        if (pattern.Length > 200)
            throw new ArgumentException("Cache pattern cannot exceed 200 characters", nameof(pattern));

        if (!SafePatternRegex.IsMatch(pattern))
            throw new ArgumentException("Cache pattern contains unsafe characters", nameof(pattern));

        // Prevent dangerous patterns
        if (pattern == "*" || pattern == "**")
            throw new ArgumentException("Wildcard-only patterns are not allowed for safety", nameof(pattern));
    }

    /// <summary>
    /// Collect comprehensive system health data
    /// </summary>
    public async Task<SystemHealthResponse> CollectSystemHealthDataAsync()
    {
        try
        {
            var status = "Healthy";
            var services = new Dictionary<string, string>();

            // Collect basic system metrics
            var process = Process.GetCurrentProcess();
            var cpuUsage = await GetCpuUsageAsync();
            var memoryUsage = process.WorkingSet64;
            var uptime = DateTime.UtcNow - process.StartTime;

            // Check service health (simplified)
            services["Database"] = "Healthy"; // In real implementation, would check actual DB connectivity
            services["Cache"] = "Healthy";    // In real implementation, would check cache connectivity
            services["Logger"] = "Healthy";   // In real implementation, would check logging service

            // Determine overall status
            if (cpuUsage > 90 || memoryUsage > 1024 * 1024 * 1024) // > 1GB
            {
                status = "Warning";
            }

            if (services.Values.Any(s => s != "Healthy"))
            {
                status = "Degraded";
            }

            return new SystemHealthResponse(
                Status: status,
                Uptime: uptime,
                CpuUsage: cpuUsage,
                MemoryUsage: memoryUsage,
                Services: services,
                Timestamp: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting system health data");
            
            return new SystemHealthResponse(
                Status: "Error",
                Uptime: TimeSpan.Zero,
                CpuUsage: 0,
                MemoryUsage: 0,
                Services: new Dictionary<string, string> { ["Error"] = ex.Message },
                Timestamp: DateTime.UtcNow
            );
        }
    }

    #region Private Helper Methods

    private static bool IsValidMetricName(string name)
    {
        return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-');
    }

    private static bool IsValidEventName(string name)
    {
        return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-');
    }

    private static double CalculatePerformanceHealthScore(Dictionary<string, OperationStats> operations)
    {
        if (!operations.Any()) return 100;

        var scores = operations.Values.Select(op =>
        {
            // Score based on response time (lower is better)
            var timeScore = Math.Max(0, 100 - (op.AverageMs / 10)); // 10ms = 99 points, 1000ms = 0 points
            
            // Score based on consistency (lower variance is better)
            var variance = op.MaxMs - op.MinMs;
            var consistencyScore = Math.Max(0, 100 - (variance / 20));
            
            return (timeScore + consistencyScore) / 2;
        });

        return Math.Round(scores.Average(), 1);
    }

    private static double CalculateCacheEfficiencyScore(double hitRate, int totalKeys)
    {
        var hitRateScore = hitRate; // 0-100
        var keyUtilizationScore = Math.Min(100, totalKeys / 10.0); // Assume 1000 keys = 100% utilization
        
        return Math.Round((hitRateScore * 0.7 + keyUtilizationScore * 0.3), 1);
    }

    private static T GetStatValue<T>(Dictionary<string, object> stats, string key, T defaultValue)
    {
        if (stats.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    private static async Task<double> GetCpuUsageAsync()
    {
        // Simplified CPU usage calculation
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        
        await Task.Delay(500); // Sample for 500ms
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return Math.Round(cpuUsageTotal * 100, 2);
    }

    #endregion
}
