using StackExchange.Redis;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Redis service implementation for pattern operations and direct Redis access
/// </summary>
public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private readonly ILogger<RedisService> _logger;
    private readonly IDatabase? _database;

    public RedisService(IConnectionMultiplexer? connectionMultiplexer, ILogger<RedisService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
        _database = _connectionMultiplexer?.GetDatabase();
    }

    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_database == null || !IsConnected())
        {
            _logger.LogWarning("Redis not available for pattern removal: {Pattern}", pattern);
            return 0;
        }

        try
        {
            var removedCount = 0;
            var server = GetServer();
            if (server == null) return 0;

            // Use SCAN to safely iterate through keys (better than KEYS *)
            var keys = server.Keys(pattern: ConvertPatternToRedisPattern(pattern), pageSize: 1000);
            
            // Remove keys in batches to avoid blocking Redis
            var keyBatch = new List<RedisKey>();
            foreach (var key in keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                keyBatch.Add(key);
                
                // Process in batches of 100
                if (keyBatch.Count >= 100)
                {
                    var deletedInBatch = await _database.KeyDeleteAsync(keyBatch.ToArray());
                    removedCount += (int)deletedInBatch;
                    keyBatch.Clear();
                    
                    // Small delay to prevent overwhelming Redis
                    await Task.Delay(1, cancellationToken);
                }
            }

            // Process remaining keys
            if (keyBatch.Count > 0)
            {
                var deletedInBatch = await _database.KeyDeleteAsync(keyBatch.ToArray());
                removedCount += (int)deletedInBatch;
            }

            _logger.LogInformation("Removed {Count} keys matching pattern: {Pattern}", removedCount, pattern);
            return removedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys by pattern: {Pattern}", pattern);
            return 0;
        }
    }

    public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_database == null || !IsConnected())
        {
            _logger.LogWarning("Redis not available for pattern search: {Pattern}", pattern);
            return Enumerable.Empty<string>();
        }

        try
        {
            var server = GetServer();
            if (server == null) return Enumerable.Empty<string>();

            var keys = new List<string>();
            var redisKeys = server.Keys(pattern: ConvertPatternToRedisPattern(pattern), pageSize: 1000);
            
            foreach (var key in redisKeys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                keys.Add(key.ToString());
                
                // Limit to prevent memory issues
                if (keys.Count >= 10000)
                {
                    _logger.LogWarning("Key search limited to 10,000 results for pattern: {Pattern}", pattern);
                    break;
                }
            }

            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting keys by pattern: {Pattern}", pattern);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
            return false;

        try
        {
            if (_database == null) return false;
            
            // Simple ping test
            var pingResult = await _database.PingAsync();
            return pingResult.TotalMilliseconds < 1000; // Consider available if ping < 1s
        }
        catch
        {
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetInfoAsync()
    {
        var info = new Dictionary<string, object>
        {
            ["Connected"] = IsConnected(),
            ["Available"] = false,
            ["ServerInfo"] = "N/A"
        };

        if (_database == null || !IsConnected())
            return info;

        try
        {
            info["Available"] = await IsAvailableAsync();
            
            var server = GetServer();
            if (server != null)
            {
                var serverInfo = await server.InfoAsync("server");
                info["ServerInfo"] = serverInfo.FirstOrDefault()?.ToString() ?? "N/A";
            }
        }
        catch (Exception ex)
        {
            info["Error"] = ex.Message;
        }

        return info;
    }

    private bool IsConnected()
    {
        return _connectionMultiplexer?.IsConnected ?? false;
    }

    private IServer? GetServer()
    {
        try
        {
            return _connectionMultiplexer?.GetServer(_connectionMultiplexer.GetEndPoints().First());
        }
        catch
        {
            return null;
        }
    }

    private static string ConvertPatternToRedisPattern(string pattern)
    {
        // Convert simple wildcard patterns to Redis patterns
        // * becomes * (same)
        // ? becomes ? (same)
        // flights:* becomes flights:*
        return pattern;
    }
}
