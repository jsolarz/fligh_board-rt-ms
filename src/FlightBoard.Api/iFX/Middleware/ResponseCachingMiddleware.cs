namespace FlightBoard.Api.iFX.Middleware;

/// <summary>
/// Custom middleware for adding response caching headers based on endpoint and content type
/// </summary>
public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add cache headers before processing
        AddCacheHeaders(context);

        await _next(context);

        // Add final cache headers after processing
        FinalizeHeaders(context);
    }

    private void AddCacheHeaders(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method;

        // Only apply caching to GET requests
        if (!HttpMethods.IsGet(method))
        {
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
            return;
        }

        // Determine cache duration based on endpoint
        var cacheDuration = GetCacheDuration(path);
        
        if (cacheDuration > 0)
        {
            // Add caching headers
            context.Response.Headers.CacheControl = $"public, max-age={cacheDuration}";
            context.Response.Headers.Expires = DateTime.UtcNow.AddSeconds(cacheDuration).ToString("R");
            
            // Add ETag support for better caching
            var etag = GenerateETag(context.Request);
            if (!string.IsNullOrEmpty(etag))
            {
                context.Response.Headers.ETag = etag;
                
                // Check if client has cached version
                var ifNoneMatch = context.Request.Headers.IfNoneMatch.ToString();
                if (ifNoneMatch == etag)
                {
                    context.Response.StatusCode = 304; // Not Modified
                    return;
                }
            }
        }
        else
        {
            // No caching for dynamic content
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Response.Headers.Pragma = "no-cache";
        }
    }

    private void FinalizeHeaders(HttpContext context)
    {
        // Add additional performance headers
        if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        }

        if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
        {
            context.Response.Headers.Add("X-Frame-Options", "DENY");
        }

        if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
        {
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        }

        // Add server timing header for performance monitoring
        if (context.Items.TryGetValue("ServerTiming", out var timing))
        {
            context.Response.Headers.Add("Server-Timing", timing.ToString());
        }
    }

    private static int GetCacheDuration(string path)
    {
        return path switch
        {
            // Static data - longer cache times
            var p when p.Contains("/api/flights/search") => 300, // 5 minutes
            var p when p.Contains("/api/performance/cache/analytics") => 60, // 1 minute
            var p when p.Contains("/api/performance/cache/stats") => 30, // 30 seconds
            
            // Semi-static data - medium cache times
            var p when p.Contains("/api/flights") && !p.Contains("create") && !p.Contains("update") && !p.Contains("delete") => 120, // 2 minutes
            
            // Health checks - short cache
            var p when p.Contains("/health") => 30, // 30 seconds
            
            // Dynamic data - no cache
            var p when p.Contains("signalr") || p.Contains("hub") => 0,
            var p when p.Contains("create") || p.Contains("update") || p.Contains("delete") => 0,
            var p when p.Contains("admin") || p.Contains("clear") => 0,
            
            // Default for API endpoints
            var p when p.StartsWith("/api/") => 60, // 1 minute default
            
            // No cache for everything else
            _ => 0
        };
    }

    private static string GenerateETag(HttpRequest request)
    {
        // Generate ETag based on URL and query parameters
        var url = $"{request.Path}{request.QueryString}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url));
        return $"\"{Convert.ToHexString(hash)[..16]}\"";
    }
}
