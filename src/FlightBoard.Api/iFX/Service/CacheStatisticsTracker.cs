using FlightBoard.Api.iFX.Contract.Service;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// High-performance cache statistics tracker with thread-safe operations
/// </summary>
public class CacheStatisticsTracker : ICacheStatisticsTracker
{
    private readonly object _lockObject = new();
    private readonly DateTime _startTime;
    private readonly ILogger<CacheStatisticsTracker> _logger;

    // Memory layer statistics
    private long _memoryHits;
    private long _memoryMisses;
    private long _memorySets;
    private long _memoryRemoves;
    private readonly ConcurrentDictionary<string, bool> _memoryKeys = new();
    private readonly ConcurrentQueue<double> _memoryResponseTimes = new();
    private long _memoryTotalBytes;

    // Redis layer statistics
    private long _redisHits;
    private long _redisMisses;
    private long _redisSets;
    private long _redisRemoves;
    private readonly ConcurrentQueue<double> _redisResponseTimes = new();
    private long _redisTotalBytes;

    // Combined layer statistics
    private long _combinedHits;
    private long _combinedMisses;
    private long _combinedSets;
    private long _combinedRemoves;

    // Operation-specific statistics
    private readonly ConcurrentDictionary<string, OperationTracker> _operationTrackers = new();

    public CacheStatisticsTracker(ILogger<CacheStatisticsTracker> logger)
    {
        _logger = logger;
        _startTime = DateTime.UtcNow;
    }

    public void TrackHit(string key, double responseTimeMs, CacheLayer layer)
    {
        switch (layer)
        {
            case CacheLayer.Memory:
                Interlocked.Increment(ref _memoryHits);
                AddResponseTime(_memoryResponseTimes, responseTimeMs);
                break;
            case CacheLayer.Redis:
                Interlocked.Increment(ref _redisHits);
                AddResponseTime(_redisResponseTimes, responseTimeMs);
                break;
            case CacheLayer.Combined:
                Interlocked.Increment(ref _combinedHits);
                break;
        }

        TrackOperation("Get", responseTimeMs, layer);
        _logger.LogDebug("Cache hit tracked: {Layer} - {Key} - {ResponseTime}ms", layer, key, responseTimeMs);
    }

    public void TrackMiss(string key, double responseTimeMs, CacheLayer layer)
    {
        switch (layer)
        {
            case CacheLayer.Memory:
                Interlocked.Increment(ref _memoryMisses);
                AddResponseTime(_memoryResponseTimes, responseTimeMs);
                break;
            case CacheLayer.Redis:
                Interlocked.Increment(ref _redisMisses);
                AddResponseTime(_redisResponseTimes, responseTimeMs);
                break;
            case CacheLayer.Combined:
                Interlocked.Increment(ref _combinedMisses);
                break;
        }

        TrackOperation("Get", responseTimeMs, layer);
        _logger.LogDebug("Cache miss tracked: {Layer} - {Key} - {ResponseTime}ms", layer, key, responseTimeMs);
    }

    public void TrackSet(string key, double responseTimeMs, CacheLayer layer, int valueSizeBytes = 0)
    {
        switch (layer)
        {
            case CacheLayer.Memory:
                Interlocked.Increment(ref _memorySets);
                AddResponseTime(_memoryResponseTimes, responseTimeMs);
                _memoryKeys.TryAdd(key, true);
                Interlocked.Add(ref _memoryTotalBytes, valueSizeBytes);
                break;
            case CacheLayer.Redis:
                Interlocked.Increment(ref _redisSets);
                AddResponseTime(_redisResponseTimes, responseTimeMs);
                Interlocked.Add(ref _redisTotalBytes, valueSizeBytes);
                break;
            case CacheLayer.Combined:
                Interlocked.Increment(ref _combinedSets);
                break;
        }

        TrackOperation("Set", responseTimeMs, layer, valueSizeBytes);
        _logger.LogDebug("Cache set tracked: {Layer} - {Key} - {ResponseTime}ms - {Size}bytes", 
            layer, key, responseTimeMs, valueSizeBytes);
    }

    public void TrackRemove(string key, double responseTimeMs, CacheLayer layer)
    {
        switch (layer)
        {
            case CacheLayer.Memory:
                Interlocked.Increment(ref _memoryRemoves);
                AddResponseTime(_memoryResponseTimes, responseTimeMs);
                _memoryKeys.TryRemove(key, out _);
                break;
            case CacheLayer.Redis:
                Interlocked.Increment(ref _redisRemoves);
                AddResponseTime(_redisResponseTimes, responseTimeMs);
                break;
            case CacheLayer.Combined:
                Interlocked.Increment(ref _combinedRemoves);
                break;
        }

        TrackOperation("Remove", responseTimeMs, layer);
        _logger.LogDebug("Cache remove tracked: {Layer} - {Key} - {ResponseTime}ms", layer, key, responseTimeMs);
    }

    public void TrackPatternOperation(string pattern, int keysAffected, double responseTimeMs, CacheLayer layer)
    {
        TrackOperation("PatternRemove", responseTimeMs, layer, keysAffected);
        _logger.LogInformation("Pattern operation tracked: {Layer} - {Pattern} - {KeysAffected} keys - {ResponseTime}ms", 
            layer, pattern, keysAffected, responseTimeMs);
    }

    public CacheStatistics GetStatistics()
    {
        var uptime = DateTime.UtcNow - _startTime;
        
        var memoryStats = new CacheLayerStats(
            TotalHits: _memoryHits,
            TotalMisses: _memoryMisses,
            TotalSets: _memorySets,
            TotalRemoves: _memoryRemoves,
            TotalRequests: _memoryHits + _memoryMisses,
            HitRatePercent: CalculateHitRate(_memoryHits, _memoryMisses),
            AverageResponseTimeMs: CalculateAverageResponseTime(_memoryResponseTimes),
            MinResponseTimeMs: CalculateMinResponseTime(_memoryResponseTimes),
            MaxResponseTimeMs: CalculateMaxResponseTime(_memoryResponseTimes),
            CurrentKeyCount: _memoryKeys.Count,
            TotalBytesStored: _memoryTotalBytes,
            OperationStats: GetOperationStatsByLayer("Memory")
        );

        var redisStats = new CacheLayerStats(
            TotalHits: _redisHits,
            TotalMisses: _redisMisses,
            TotalSets: _redisSets,
            TotalRemoves: _redisRemoves,
            TotalRequests: _redisHits + _redisMisses,
            HitRatePercent: CalculateHitRate(_redisHits, _redisMisses),
            AverageResponseTimeMs: CalculateAverageResponseTime(_redisResponseTimes),
            MinResponseTimeMs: CalculateMinResponseTime(_redisResponseTimes),
            MaxResponseTimeMs: CalculateMaxResponseTime(_redisResponseTimes),
            CurrentKeyCount: 0, // Would need Redis-specific tracking
            TotalBytesStored: _redisTotalBytes,
            OperationStats: GetOperationStatsByLayer("Redis")
        );

        var combinedStats = new CacheLayerStats(
            TotalHits: _combinedHits,
            TotalMisses: _combinedMisses,
            TotalSets: _combinedSets,
            TotalRemoves: _combinedRemoves,
            TotalRequests: _combinedHits + _combinedMisses,
            HitRatePercent: CalculateHitRate(_combinedHits, _combinedMisses),
            AverageResponseTimeMs: 0, // Combined doesn't have its own timing
            MinResponseTimeMs: 0,
            MaxResponseTimeMs: 0,
            CurrentKeyCount: _memoryKeys.Count, // Best estimate
            TotalBytesStored: _memoryTotalBytes + _redisTotalBytes,
            OperationStats: GetOperationStatsByLayer("Combined")
        );

        var additionalMetrics = new Dictionary<string, object>
        {
            ["MemoryEfficiency"] = CalculateMemoryEfficiency(),
            ["RedisEfficiency"] = CalculateRedisEfficiency(),
            ["OverallEfficiency"] = CalculateOverallEfficiency(),
            ["AverageValueSize"] = CalculateAverageValueSize(),
            ["OperationsPerSecond"] = CalculateOperationsPerSecond(uptime),
            ["CacheLayerPreference"] = GetCacheLayerPreference()
        };

        return new CacheStatistics(
            StartTime: _startTime,
            Uptime: uptime,
            Memory: memoryStats,
            Redis: redisStats,
            Combined: combinedStats,
            AdditionalMetrics: additionalMetrics
        );
    }

    public void Reset()
    {
        lock (_lockObject)
        {
            // Reset all counters
            Interlocked.Exchange(ref _memoryHits, 0);
            Interlocked.Exchange(ref _memoryMisses, 0);
            Interlocked.Exchange(ref _memorySets, 0);
            Interlocked.Exchange(ref _memoryRemoves, 0);
            Interlocked.Exchange(ref _memoryTotalBytes, 0);

            Interlocked.Exchange(ref _redisHits, 0);
            Interlocked.Exchange(ref _redisMisses, 0);
            Interlocked.Exchange(ref _redisSets, 0);
            Interlocked.Exchange(ref _redisRemoves, 0);
            Interlocked.Exchange(ref _redisTotalBytes, 0);

            Interlocked.Exchange(ref _combinedHits, 0);
            Interlocked.Exchange(ref _combinedMisses, 0);
            Interlocked.Exchange(ref _combinedSets, 0);
            Interlocked.Exchange(ref _combinedRemoves, 0);

            // Clear collections
            _memoryKeys.Clear();
            ClearResponseTimes(_memoryResponseTimes);
            ClearResponseTimes(_redisResponseTimes);
            _operationTrackers.Clear();

            _logger.LogInformation("Cache statistics reset");
        }
    }

    #region Private Helper Methods

    private void TrackOperation(string operationType, double responseTimeMs, CacheLayer layer, int dataSize = 0)
    {
        var key = $"{layer}_{operationType}";
        _operationTrackers.AddOrUpdate(key,
            new OperationTracker(1, responseTimeMs, responseTimeMs, responseTimeMs, dataSize),
            (_, existing) => existing.Update(responseTimeMs, dataSize));
    }

    private static double CalculateHitRate(long hits, long misses)
    {
        var total = hits + misses;
        return total > 0 ? Math.Round((double)hits / total * 100, 2) : 0;
    }

    private static void AddResponseTime(ConcurrentQueue<double> queue, double responseTimeMs)
    {
        queue.Enqueue(responseTimeMs);
        // Keep only last 1000 measurements to prevent memory growth
        while (queue.Count > 1000)
        {
            queue.TryDequeue(out _);
        }
    }

    private static double CalculateAverageResponseTime(ConcurrentQueue<double> responseTimes)
    {
        var times = responseTimes.ToArray();
        return times.Length > 0 ? Math.Round(times.Average(), 2) : 0;
    }

    private static double CalculateMinResponseTime(ConcurrentQueue<double> responseTimes)
    {
        var times = responseTimes.ToArray();
        return times.Length > 0 ? times.Min() : 0;
    }

    private static double CalculateMaxResponseTime(ConcurrentQueue<double> responseTimes)
    {
        var times = responseTimes.ToArray();
        return times.Length > 0 ? times.Max() : 0;
    }

    private Dictionary<string, CacheOperationStats> GetOperationStatsByLayer(string layer)
    {
        return _operationTrackers
            .Where(kvp => kvp.Key.StartsWith(layer))
            .ToDictionary(
                kvp => kvp.Key.Substring(layer.Length + 1), // Remove "Layer_" prefix
                kvp => kvp.Value.ToCacheOperationStats()
            );
    }

    private double CalculateMemoryEfficiency()
    {
        var hitRate = CalculateHitRate(_memoryHits, _memoryMisses);
        var avgResponseTime = CalculateAverageResponseTime(_memoryResponseTimes);
        // Efficiency based on hit rate (70%) and speed (30%)
        var speedScore = Math.Max(0, 100 - (avgResponseTime / 10)); // 10ms = 90 points
        return Math.Round(hitRate * 0.7 + speedScore * 0.3, 1);
    }

    private double CalculateRedisEfficiency()
    {
        var hitRate = CalculateHitRate(_redisHits, _redisMisses);
        var avgResponseTime = CalculateAverageResponseTime(_redisResponseTimes);
        var speedScore = Math.Max(0, 100 - (avgResponseTime / 50)); // 50ms = 98 points
        return Math.Round(hitRate * 0.7 + speedScore * 0.3, 1);
    }

    private double CalculateOverallEfficiency()
    {
        var memoryEff = CalculateMemoryEfficiency();
        var redisEff = CalculateRedisEfficiency();
        return Math.Round((memoryEff + redisEff) / 2, 1);
    }

    private double CalculateAverageValueSize()
    {
        var totalSets = _memorySets + _redisSets;
        var totalBytes = _memoryTotalBytes + _redisTotalBytes;
        return totalSets > 0 ? Math.Round((double)totalBytes / totalSets, 1) : 0;
    }

    private double CalculateOperationsPerSecond(TimeSpan uptime)
    {
        var totalOps = _memoryHits + _memoryMisses + _memorySets + _memoryRemoves +
                      _redisHits + _redisMisses + _redisSets + _redisRemoves;
        return uptime.TotalSeconds > 0 ? Math.Round(totalOps / uptime.TotalSeconds, 2) : 0;
    }

    private string GetCacheLayerPreference()
    {
        var memoryTotal = _memoryHits + _memoryMisses;
        var redisTotal = _redisHits + _redisMisses;
        
        if (memoryTotal > redisTotal * 2) return "Memory-Heavy";
        if (redisTotal > memoryTotal * 2) return "Redis-Heavy";
        return "Balanced";
    }

    private static void ClearResponseTimes(ConcurrentQueue<double> queue)
    {
        while (queue.TryDequeue(out _)) { }
    }

    #endregion
}

/// <summary>
/// Internal operation tracker for statistics
/// </summary>
internal class OperationTracker
{
    private long _count;
    private double _totalMs;
    private double _minMs;
    private double _maxMs;
    private long _totalBytes;

    public OperationTracker(long count, double totalMs, double minMs, double maxMs, long totalBytes)
    {
        _count = count;
        _totalMs = totalMs;
        _minMs = minMs;
        _maxMs = maxMs;
        _totalBytes = totalBytes;
    }

    public OperationTracker Update(double responseTimeMs, int dataSize = 0)
    {
        Interlocked.Increment(ref _count);
        
        lock (this)
        {
            _totalMs += responseTimeMs;
            _minMs = Math.Min(_minMs, responseTimeMs);
            _maxMs = Math.Max(_maxMs, responseTimeMs);
            _totalBytes += dataSize;
        }
        
        return this;
    }

    public CacheOperationStats ToCacheOperationStats()
    {
        return new CacheOperationStats(
            Count: _count,
            AverageMs: _count > 0 ? Math.Round(_totalMs / _count, 2) : 0,
            MinMs: _minMs,
            MaxMs: _maxMs,
            TotalBytes: _totalBytes
        );
    }
}
