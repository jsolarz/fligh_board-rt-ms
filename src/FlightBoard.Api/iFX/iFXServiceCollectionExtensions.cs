using FlightBoard.Api.iFX.CrossCutting.Notifications;
using FlightBoard.Api.iFX.Utilities.Mapping;

namespace FlightBoard.Api.iFX;

/// <summary>
/// Extension methods for registering iFX services
/// Following dependency injection best practices
/// </summary>
public static class iFXServiceCollectionExtensions
{
    /// <summary>
    /// Register all iFX cross-cutting concerns and utilities
    /// </summary>
    public static IServiceCollection AddiFXServices(this IServiceCollection services)
    {
        // Register cross-cutting concerns
        services.AddCrossCuttingServices();

        // Register utilities
        services.AddUtilityServices();

        return services;
    }

    /// <summary>
    /// Register cross-cutting concern services
    /// </summary>
    private static IServiceCollection AddCrossCuttingServices(this IServiceCollection services)
    {
        // Notification engines
        services.AddScoped<INotificationEngine, SignalRNotificationEngine>();

        return services;
    }

    /// <summary>
    /// Register utility services
    /// </summary>
    private static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        // Mapping utilities
        services.AddScoped<IFlightMappingUtility, FlightMappingUtility>();

        return services;
    }
}
