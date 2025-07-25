using FlightBoard.Api.Managers;
using FlightBoard.Api.Engines;
using FlightBoard.Api.DataAccess.Flight;
using FlightBoard.Api.DataAccess.User;
using FlightBoard.Api.Contract.Flight;
using FlightBoard.Api.Contract.Auth;
using FlightBoard.Api.Contract.Performance;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.iFX.Contract;
using FlightBoard.Api.iFX.Service;
using FlightBoard.Api.iFX;
using FlightBoard.Api.iFX.Engines;

namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring application layer services
/// </summary>
public static class ApplicationServicesConfiguration
{
    /// <summary>
    /// Add application services following iDesign Method layering
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Data Access layer
        services.AddScoped<IFlightDataAccess, FlightDataAccess>();
        services.AddScoped<IUserDataAccess, UserDataAccess>();

        // Engine layer (pure business logic)
        services.AddScoped<IFlightEngine, FlightEngine>();
        services.AddScoped<IAuthEngine, AuthEngine>();
        services.AddScoped<IPerformanceEngine, PerformanceEngine>();

        // Manager layer (use case orchestration)
        services.AddScoped<FlightManager>();
        services.AddScoped<IFlightManager>(provider =>
        {
            var baseManager = provider.GetRequiredService<FlightManager>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            var perfService = provider.GetRequiredService<IPerformanceService>();
            var logger = provider.GetRequiredService<ILogger<FlightBoard.Api.Managers.CachedFlightManager>>();
            return new FlightBoard.Api.Managers.CachedFlightManager(baseManager, cacheService, perfService, logger);
        });
        services.AddScoped<IAuthManager, AuthManager>();
        services.AddScoped<IPerformanceManager, PerformanceManager>();

        // Infrastructure services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IPerformanceService, PerformanceService>();

        // iFX Framework services
        services.AddiFXServices();

        return services;
    }
}
