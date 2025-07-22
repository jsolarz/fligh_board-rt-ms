using System.Diagnostics;

namespace FlightBoard.Api.iFX.Contract.Service;

/// <summary>
/// Performance monitoring contract for tracking application metrics
/// Provides insights into application performance and bottlenecks
/// </summary>
public interface IPerformanceService
{
    /// <summary>
    /// Start tracking an operation
    /// </summary>
    IDisposable TrackOperation(string operationName);
    
    /// <summary>
    /// Track metric value
    /// </summary>
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    
    /// <summary>
    /// Track custom event
    /// </summary>
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);
    
    /// <summary>
    /// Get performance summary
    /// </summary>
    Task<PerformanceSummary> GetSummaryAsync();
}

/// <summary>
/// Performance summary data
/// </summary>
public record PerformanceSummary(
    Dictionary<string, OperationStats> Operations,
    Dictionary<string, double> Metrics,
    TimeSpan Uptime,
    DateTime LastReset
);

/// <summary>
/// Operation statistics
/// </summary>
public record OperationStats(
    long Count,
    double AverageMs,
    double MinMs,
    double MaxMs,
    double TotalMs
);
