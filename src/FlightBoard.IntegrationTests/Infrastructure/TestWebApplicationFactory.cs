using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FlightBoard.Api.Data;

namespace FlightBoard.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Sets up in-memory database and test-specific configurations
/// </summary>
/// <typeparam name="TProgram">Program class from the main application</typeparam>
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related services first
            var toRemove = services.Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<FlightDbContext>) ||
                descriptor.ServiceType == typeof(FlightDbContext) ||
                descriptor.ServiceType.Name.Contains("DbContext")).ToList();
            
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Add fresh in-memory database for testing
            services.AddDbContext<FlightDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDatabase");
                options.EnableSensitiveDataLogging();
            });

            // Override logging for cleaner test output
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise
            });
        });

        builder.UseEnvironment("Testing");
    }
}
