using Microsoft.Extensions.DependencyInjection;
using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Net.Http;
using FlightBoard.Api.iFX.Contract.Service;

namespace FlightBoard.IntegrationTests.Infrastructure;

/// <summary>
/// Helper class for integration test utilities
/// Provides common functionality for test setup and data seeding
/// </summary>
public static class TestUtilities
{
    /// <summary>
    /// Seeds the test database with sample flight data
    /// </summary>
    public static async Task SeedTestDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();

        // Clear existing data
        context.Flights.RemoveRange(context.Flights);
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        // Add test users
        var testUsers = new[]
        {
            new User
            {
                Username = "testadmin",
                Email = "admin@test.com",
                PasswordHash = "hashed_password_123",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "testoperator",
                Email = "operator@test.com", 
                PasswordHash = "hashed_password_456",
                Role = UserRole.Operator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(testUsers);

        // Add test flights
        var testFlights = new[]
        {
            new Flight
            {
                FlightNumber = "AA123",
                Airline = "American Airlines",
                Origin = "JFK",
                Destination = "LAX",
                ScheduledDeparture = DateTime.UtcNow.AddHours(2),
                ScheduledArrival = DateTime.UtcNow.AddHours(5),
                Gate = "A12",
                Terminal = "1",
                Status = FlightStatus.Scheduled,
                Type = FlightType.Departure,
                AircraftType = "Boeing 737",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "DL456",
                Airline = "Delta Airlines",
                Origin = "ATL",
                Destination = "MIA",
                ScheduledDeparture = DateTime.UtcNow.AddMinutes(15),
                ScheduledArrival = DateTime.UtcNow.AddHours(3),
                Gate = "B5",
                Terminal = "2",
                Status = FlightStatus.Boarding,
                Type = FlightType.Departure,
                AircraftType = "Airbus A320",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "UA789",
                Airline = "United Airlines",
                Origin = "DEN",
                Destination = "ORD",
                ScheduledDeparture = DateTime.UtcNow.AddMinutes(-30),
                ScheduledArrival = DateTime.UtcNow.AddHours(1),
                ActualDeparture = DateTime.UtcNow.AddMinutes(-25),
                Gate = "C8",
                Terminal = "3",
                Status = FlightStatus.Departed,
                Type = FlightType.Departure,
                AircraftType = "Boeing 777",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Flights.AddRangeAsync(testFlights);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a valid JWT token for testing protected endpoints
    /// </summary>
    public static string GenerateTestJwtToken(string username = "testadmin", UserRole role = UserRole.Admin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DevelopmentJwtSecretKey2024ForLocalOnly"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("sub", "1"),
            new Claim("email", $"{username}@test.com")
        };

        var token = new JwtSecurityToken(
            issuer: "FlightBoard.Api",
            audience: "FlightBoard.Frontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a valid CreateFlightDto for testing
    /// </summary>
    public static CreateFlightDto CreateValidFlightDto(string? flightNumber = null)
    {
        return new CreateFlightDto
        {
            FlightNumber = flightNumber ?? $"TS{Random.Shared.Next(100, 999)}",
            Airline = "Test Airlines",
            Origin = "TEST",
            Destination = "DEST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(3),
            ScheduledArrival = DateTime.UtcNow.AddHours(6),
            Gate = "T1",
            Terminal = "Test",
            Status = "Scheduled",
            Type = "Departure",
            AircraftType = "Test Aircraft"
        };
    }

    /// <summary>
    /// Creates a valid UpdateFlightDto for testing
    /// </summary>
    public static UpdateFlightDto CreateValidUpdateFlightDto(int id)
    {
        return new UpdateFlightDto
        {
            FlightNumber = $"UP{Random.Shared.Next(100, 999)}",
            Airline = "Updated Airlines",
            Origin = "UPD1",
            Destination = "UPD2",
            ScheduledDeparture = DateTime.UtcNow.AddHours(4),
            ScheduledArrival = DateTime.UtcNow.AddHours(7),
            Gate = "U1",
            Terminal = "Updated",
            Status = "Scheduled",
            Type = "Departure",
            AircraftType = "Updated Aircraft"
        };
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    public static async Task CleanupTestDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();

        context.Flights.RemoveRange(context.Flights);
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a test user registration DTO
    /// </summary>
    public static RegisterDto CreateValidRegisterUserDto(string? email = null)
    {
        var randomId = Random.Shared.Next(100, 999);
        return new RegisterDto
        {
            Username = $"testuser{randomId}",
            Email = email ?? $"testuser{randomId}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
    }

    /// <summary>
    /// Creates a test user login DTO
    /// </summary>
    public static LoginDto CreateValidLoginUserDto(string username = "testuser123", string password = "TestPassword123!")
    {
        return new LoginDto
        {
            Username = username,
            Password = password
        };
    }
}
