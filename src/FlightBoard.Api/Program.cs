using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Hubs;
using FlightBoard.Api.Configuration;
using FlightBoard.Api.iFX.Middleware;
using FlightBoard.Api.Services;
using Serilog;

// Configure logging
LoggingConfiguration.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Configure services
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWebServices(); // This now includes performance optimizations

var app = builder.Build();

// Initialize database
await InitializeDatabaseAsync(app.Services);

// Configure middleware pipeline with performance optimizations
ConfigureMiddleware(app);

// Configure endpoints
ConfigureEndpoints(app);

app.Run();

/// <summary>
/// Initialize database with migrations and seed data
/// </summary>
static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring database is created and up to date");
        await context.Database.MigrateAsync();

        logger.LogInformation("Seeding database with initial data");
        await seeder.SeedAsync();

        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

/// <summary>
/// Configure the middleware pipeline with performance optimizations
/// </summary>
static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Apply performance optimizations early in the pipeline
    app.UsePerformanceOptimizations();

    // Custom performance monitoring and caching middleware
    app.UseMiddleware<PerformanceMonitoringMiddleware>();
    app.UseMiddleware<ResponseCachingMiddleware>();

    app.UseCors("AllowFrontends");
    app.UseAuthentication();
    app.UseAuthorization();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
}

/// <summary>
/// Configure application endpoints
/// </summary>
static void ConfigureEndpoints(WebApplication app)
{
    app.MapControllers();
    app.MapHub<FlightHub>("/flighthub");
}

// Make the Program class accessible to integration tests
public partial class Program { }

// Ensure Serilog flushes logs on application shutdown
public static class SerilogConfiguration
{
    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}
