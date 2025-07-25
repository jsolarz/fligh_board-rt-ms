using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Services;
using Serilog;
using Serilog.Events;

namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring logging services
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configure Serilog for structured logging
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("FlightBoard.Api", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FlightBoard.Api")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File("Logs/flightboard-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .CreateLogger();
    }
}

/// <summary>
/// Extension methods for configuring database services
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Add Entity Framework with SQLite configuration
    /// </summary>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? "Data Source=..\\..\\Data\\flightboard.db";

        services.AddDbContext<FlightDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<DatabaseSeeder>();
        return services;
    }
}
