namespace FlightBoard.Api.iFX.Middleware;

/// <summary>
/// Performance monitoring middleware for tracking API response times
/// Following iDesign Method: Infrastructure cross-cutting concern
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // Only log API endpoints, not static files or health checks
            if (ShouldLogRequest(path))
            {
                if (elapsedMs > 1000) // Log slow requests (>1s) as warnings
                {
                    _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms - Status: {StatusCode}",
                        context.Request.Method, path, elapsedMs, statusCode);
                }
                else if (elapsedMs > 500) // Log moderately slow requests (>500ms) as info
                {
                    _logger.LogInformation("Request: {Method} {Path} took {ElapsedMs}ms - Status: {StatusCode}",
                        context.Request.Method, path, elapsedMs, statusCode);
                }
                else
                {
                    _logger.LogDebug("Request: {Method} {Path} took {ElapsedMs}ms - Status: {StatusCode}",
                        context.Request.Method, path, elapsedMs, statusCode);
                }

                // Add custom header with response time
                context.Response.Headers.TryAdd("X-Response-Time-Ms", elapsedMs.ToString());
            }
        }
    }

    private static bool ShouldLogRequest(string path)
    {
        // Skip health checks, static files, and SignalR negotiate
        var skipPaths = new[] { "/health", "/favicon.ico", "/swagger", "/_", "/flighthub/negotiate" };
        return !skipPaths.Any(skip => path.StartsWith(skip, StringComparison.OrdinalIgnoreCase));
    }
}
