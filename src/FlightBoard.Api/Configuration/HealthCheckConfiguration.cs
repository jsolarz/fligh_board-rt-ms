using Microsoft.Extensions.Diagnostics.HealthChecks;
using FlightBoard.Api.Data;
using FlightBoard.Api.iFX.Service;

namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring comprehensive health checks
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Add comprehensive health checks for all system components
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Database health check (custom)
            .AddCheck<DatabaseHealthCheck>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "sql", "ready" })
            
            // Redis health check
            .AddCheck<RedisHealthCheck>(
                name: "redis",
                failureStatus: HealthStatus.Degraded, // Redis failure is degraded, not unhealthy
                tags: new[] { "redis", "cache", "external" })
            
            // Cache performance health check
            .AddCheck<CachePerformanceHealthCheck>(
                name: "cache-performance",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "performance" })
            
            // System resources health check
            .AddCheck<SystemResourcesHealthCheck>(
                name: "system-resources",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "system", "resources", "cpu", "memory" })
            
            // Memory health check
            .AddCheck<MemoryHealthCheck>(
                name: "memory",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "memory", "gc" });

        return services;
    }

    /// <summary>
    /// Configure health check options and endpoints
    /// </summary>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Main health endpoint
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Ready endpoint (for Kubernetes readiness probes)
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Live endpoint (for Kubernetes liveness probes)
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false, // No checks - just alive
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        return app;
    }
}
