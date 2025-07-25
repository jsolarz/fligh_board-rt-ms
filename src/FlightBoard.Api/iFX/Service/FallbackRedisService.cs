using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Fallback Redis service when Redis is not available
/// </summary>
public class FallbackRedisService : IRedisService
{
    private readonly ILogger<FallbackRedisService> _logger;

    public FallbackRedisService(ILogger<FallbackRedisService> logger)
    {
        _logger = logger;
    }

    public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Redis pattern removal not available (fallback mode): {Pattern}", pattern);
        return Task.FromResult(0);
    }

    public Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Redis pattern search not available (fallback mode): {Pattern}", pattern);
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(false);
    }

    public Task<Dictionary<string, object>> GetInfoAsync()
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            ["Connected"] = false,
            ["Available"] = false,
            ["ServerInfo"] = "Fallback Mode - Redis Not Available"
        });
    }
}
