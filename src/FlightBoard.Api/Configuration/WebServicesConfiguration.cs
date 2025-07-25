namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring web services
/// </summary>
public static class WebServicesConfiguration
{
    /// <summary>
    /// Add web services (controllers, SignalR, CORS)
    /// </summary>
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();
        services.AddSignalR();
        
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
                    .AllowCredentials();
            });
        });

        return services;
    }
}
