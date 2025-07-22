using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FlightBoard.Api.Data;
using FlightBoard.Api.Services;
using FlightBoard.Api.Hubs;
using FlightBoard.Api.Managers;
using FlightBoard.Api.Engines;
using FlightBoard.Api.DataAccess.Flight;
using FlightBoard.Api.DataAccess.User;
using FlightBoard.Api.Contract.Flight;
using FlightBoard.Api.Contract.Auth;
using FlightBoard.Api.iFX;
using FlightBoard.Api.iFX.Contract;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.iFX.Service;
using FlightBoard.Api.iFX.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<FlightDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=..\\..\\Data\\flightboard.db";
    options.UseSqlite(connectionString);

    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // No clock skew tolerance
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/flighthub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Register database seeder
builder.Services.AddScoped<DatabaseSeeder>();

// Register iDesign Method components following proper layering

// Managers (Use case orchestration layer) - using public contract interface
builder.Services.AddScoped<IFlightManager, FlightManager>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

// Engines (Business logic layer)
builder.Services.AddScoped<IFlightEngine, FlightEngine>();
builder.Services.AddScoped<IAuthEngine, AuthEngine>();

// Data Access layer - following iDesign Method naming
builder.Services.AddScoped<IFlightDataAccess, FlightDataAccess>();
builder.Services.AddScoped<IUserDataAccess, UserDataAccess>();

// iFX Framework Services (Cross-cutting concerns and utilities)
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();

// Add memory cache for basic caching
builder.Services.AddMemoryCache();

builder.Services.AddiFXServices();

// Legacy service registration (maintain for backwards compatibility during transition)
builder.Services.AddScoped<FlightService>();

// Add API controllers
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS for frontend applications
builder.Services.AddCors(options =>
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

var app = builder.Build();

// Initialize database
await InitializeDatabaseAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add performance monitoring middleware early in pipeline
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Use CORS
app.UseCors("AllowFrontends");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

// Map API controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<FlightHub>("/flighthub");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
