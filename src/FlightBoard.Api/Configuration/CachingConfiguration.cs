using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.iFX.Service;
using StackExchange.Redis;
using Serilog;

namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring caching services
/// </summary>
public static class CachingConfiguration
{
    /// <summary>
    /// Add caching services with Redis fallback to memory cache and comprehensive statistics tracking
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        // Register cache statistics tracker
        services.AddSingleton<ICacheStatisticsTracker, CacheStatisticsTracker>();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                // Add Redis connection multiplexer for direct Redis operations
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    try
                    {
                        return ConnectionMultiplexer.Connect(redisConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to connect to Redis, using fallback");
                        return null!;
                    }
                });

                // Add distributed cache (Redis) 
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "FlightBoard";
                });
                
                // Register Redis services
                services.AddScoped<ICacheService, CacheService>();
                services.AddScoped<IRedisService, RedisService>();
                
                Log.Information("Redis cache configured with connection: {RedisConnection}", redisConnectionString);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to configure Redis cache, falling back to memory cache");
                services.AddScoped<ICacheService, FallbackMemoryCacheService>();
                services.AddScoped<IRedisService, FallbackRedisService>();
            }
        }
        else
        {
            Log.Information("No Redis connection string configured, using memory cache");
            services.AddScoped<ICacheService, FallbackMemoryCacheService>();
            services.AddScoped<IRedisService, FallbackRedisService>();
        }

        return services;
    }
}
