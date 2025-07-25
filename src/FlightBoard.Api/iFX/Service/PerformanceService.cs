using FlightBoard.Api.iFX.Contract.Service;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Performance monitoring service for tracking application metrics and operations
/// Thread-safe implementation using concurrent collections
/// </summary>
public class PerformanceService : IPerformanceService
{
    private readonly ILogger<PerformanceService> _logger;
    private readonly ConcurrentDictionary<string, List<double>> _operationTimes = new();
    private readonly ConcurrentDictionary<string, double> _metrics = new();
    private readonly ConcurrentDictionary<string, long> _eventCounts = new();
    private readonly DateTime _startTime = DateTime.UtcNow;

    public PerformanceService(ILogger<PerformanceService> logger)
    {
        _logger = logger;
    }

    public IDisposable TrackOperation(string operationName)
    {
        return new OperationTracker(operationName, this);
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        _metrics.AddOrUpdate(metricName, value, (key, oldValue) => value);
        
        var propertiesLog = properties?.Count > 0 
            ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))
            : "none";
        
        _logger.LogDebug("Metric tracked: {MetricName} = {Value} (properties: {Properties})", 
            metricName, value, propertiesLog);
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        _eventCounts.AddOrUpdate(eventName, 1, (key, count) => count + 1);
        
        var propertiesLog = properties?.Count > 0 
            ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))
            : "none";
            
        _logger.LogDebug("Event tracked: {EventName} (count: {Count}, properties: {Properties})", 
            eventName, _eventCounts[eventName], propertiesLog);
    }

    public Task<PerformanceSummary> GetSummaryAsync()
    {
        var operationStats = new Dictionary<string, OperationStats>();
        
        foreach (var kvp in _operationTimes)
        {
            var times = kvp.Value;
            if (times.Count > 0)
            {
                operationStats[kvp.Key] = new OperationStats(
                    Count: times.Count,
                    AverageMs: times.Average(),
                    MinMs: times.Min(),
                    MaxMs: times.Max(),
                    TotalMs: times.Sum()
                );
            }
        }

        var summary = new PerformanceSummary(
            Operations: operationStats,
            Metrics: new Dictionary<string, double>(_metrics),
            Uptime: DateTime.UtcNow - _startTime,
            LastReset: _startTime
        );

        return Task.FromResult(summary);
    }

    internal void RecordOperationTime(string operationName, double milliseconds)
    {
        _operationTimes.AddOrUpdate(
            operationName, 
            new List<double> { milliseconds },
            (key, list) =>
            {
                lock (list)
                {
                    list.Add(milliseconds);
                    // Keep only last 1000 measurements to prevent memory issues
                    if (list.Count > 1000)
                    {
                        list.RemoveRange(0, list.Count - 1000);
                    }
                    return list;
                }
            });
    }

    private class OperationTracker : IDisposable
    {
        private readonly string _operationName;
        private readonly PerformanceService _service;
        private readonly Stopwatch _stopwatch;

        public OperationTracker(string operationName, PerformanceService service)
        {
            _operationName = operationName;
            _service = service;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _service.RecordOperationTime(_operationName, _stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
