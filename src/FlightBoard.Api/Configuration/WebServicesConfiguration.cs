namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring web services
/// </summary>
public static class WebServicesConfiguration
{
    /// <summary>
    /// Add web services (controllers, SignalR, CORS) with performance optimizations
    /// </summary>
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        
        // Add controllers with performance optimizations
        services.AddControllers(options =>
        {
            // Add cache profiles for different endpoint types
            options.CacheProfiles.Add("Default", new Microsoft.AspNetCore.Mvc.CacheProfile
            {
                Duration = 60,
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                VaryByHeader = "Accept-Encoding"
            });
            
            options.CacheProfiles.Add("Long", new Microsoft.AspNetCore.Mvc.CacheProfile
            {
                Duration = 300, // 5 minutes
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                VaryByHeader = "Accept-Encoding"
            });
            
            options.CacheProfiles.Add("Short", new Microsoft.AspNetCore.Mvc.CacheProfile
            {
                Duration = 30,
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                VaryByHeader = "Accept-Encoding"
            });
            
            options.CacheProfiles.Add("NoCache", new Microsoft.AspNetCore.Mvc.CacheProfile
            {
                NoStore = true,
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None
            });

            // Performance optimizations
            options.SuppressAsyncSuffixInActionNames = false;
            options.RespectBrowserAcceptHeader = true;
            options.ReturnHttpNotAcceptable = true;
        });
        
        // Add SignalR with performance optimizations
        services.AddSignalR(options =>
        {
            // Performance optimizations for SignalR
            options.EnableDetailedErrors = false; // Only in development
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
            options.StreamBufferCapacity = 10;
        });
        
        // Configure CORS for frontend applications
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontends", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:3000",  // Consumer frontend
                        "http://localhost:3001"   // Backoffice frontend
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Response-Time-Ms", "X-Cache-Status", "ETag"); // Expose performance headers
            });
        });

        // Add performance optimizations
        services.AddPerformanceOptimizations();

        return services;
    }
}
