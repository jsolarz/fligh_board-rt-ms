using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;
using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using FlightBoard.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlightBoard.IntegrationTests.Database;

/// <summary>
/// Integration tests for database operations
/// Tests Entity Framework Core configuration, migrations, and data persistence
/// </summary>
public class DatabaseIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private FlightDbContext _context;

    public DatabaseIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _context = null!; // Will be initialized in InitializeAsync
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await TestUtilities.CleanupTestDataAsync(_factory.Services);
        _context?.Dispose();
    }

    [Fact]
    public async Task Database_CanCreateAndRetrieveFlight()
    {
        // Arrange
        var flight = new Flight
        {
            FlightNumber = "DB001",
            Airline = "Database Airlines",
            Origin = "DBT",
            Destination = "TST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Gate = "DB1",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure,
            AircraftType = "Test Aircraft",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        var retrievedFlight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "DB001");

        // Assert
        retrievedFlight.Should().NotBeNull();
        retrievedFlight!.FlightNumber.Should().Be("DB001");
        retrievedFlight.Airline.Should().Be("Database Airlines");
        retrievedFlight.Id.Should().BeGreaterThan(0);
        retrievedFlight.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Database_EnforcesUniqueFlightNumber()
    {
        // Arrange
        var flight1 = new Flight
        {
            FlightNumber = "UQ001",
            Airline = "Unique Airlines",
            Origin = "UNQ",
            Destination = "TST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Gate = "UQ1",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure,
            CreatedAt = DateTime.UtcNow
        };

        var flight2 = new Flight
        {
            FlightNumber = "UQ001", // Same flight number
            Airline = "Duplicate Airlines",
            Origin = "DUP",
            Destination = "TST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(3),
            ScheduledArrival = DateTime.UtcNow.AddHours(6),
            Gate = "UQ2",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        _context.Flights.Add(flight1);
        await _context.SaveChangesAsync();

        _context.Flights.Add(flight2);
        
        var act = async () => await _context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Database_SoftDeleteWorks()
    {
        // Arrange
        var flight = new Flight
        {
            FlightNumber = "SD001",
            Airline = "SoftDelete Airlines",
            Origin = "SFT",
            Destination = "DEL",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Gate = "SD1",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure,
            CreatedAt = DateTime.UtcNow
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        // Act - soft delete
        flight.IsDeleted = true;
        flight.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert - flight should not appear in normal queries due to global filter
        var retrievedFlight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SD001");
        retrievedFlight.Should().BeNull();

        // But should be accessible when ignoring query filters
        var deletedFlight = await _context.Flights
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.FlightNumber == "SD001");
        
        deletedFlight.Should().NotBeNull();
        deletedFlight!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Database_AuditFieldsAutoUpdate()
    {
        // Arrange
        var flight = new Flight
        {
            FlightNumber = "AU001",
            Airline = "Audit Airlines",
            Origin = "AUD",
            Destination = "TST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Gate = "AU1",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure
            // Note: Not setting CreatedAt manually to test auto-generation
        };

        // Act - Create
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        var originalCreatedAt = flight.CreatedAt;
        originalCreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Wait a moment and update
        await Task.Delay(100);
        flight.Airline = "Updated Audit Airlines";
        await _context.SaveChangesAsync();

        // Assert
        flight.UpdatedAt.Should().BeAfter(originalCreatedAt);
        flight.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Database_IndexesImproveQueryPerformance()
    {
        // Arrange - Create multiple flights for performance testing
        var flights = new List<Flight>();
        for (int i = 0; i < 100; i++)
        {
            flights.Add(new Flight
            {
                FlightNumber = $"PF{i:D3}",
                Airline = $"Performance Airlines {i}",
                Origin = "PER",
                Destination = "TST",
                ScheduledDeparture = DateTime.UtcNow.AddHours(i % 24),
                ScheduledArrival = DateTime.UtcNow.AddHours((i % 24) + 3),
                Gate = $"P{i % 10}",
                Terminal = "Test",
                Status = (FlightStatus)(i % 4),
                Type = i % 2 == 0 ? FlightType.Departure : FlightType.Arrival,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Flights.AddRange(flights);
        await _context.SaveChangesAsync();

        // Act - Execute queries that should benefit from indexes
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var flightByNumber = await _context.Flights
            .FirstOrDefaultAsync(f => f.FlightNumber == "PF050");

        var flightsByDestination = await _context.Flights
            .Where(f => f.Destination == "TST")
            .ToListAsync();

        var flightsByStatus = await _context.Flights
            .Where(f => f.Status == FlightStatus.Scheduled)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        flightByNumber.Should().NotBeNull();
        flightsByDestination.Should().HaveCount(100);
        flightsByStatus.Count.Should().BeGreaterThan(0);
        
        // Queries should be fast (adjust threshold based on your requirements)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task Database_HandlesUserEntityCorrectly()
    {
        // Arrange
        var user = new User
        {
            Username = "dbtest",
            Email = "dbtest@test.com",
            PasswordHash = "hashedpassword123",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "dbtest");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be("dbtest");
        retrievedUser.Email.Should().Be("dbtest@test.com");
        retrievedUser.Role.Should().Be(UserRole.Operator);
        retrievedUser.IsActive.Should().BeTrue();
        retrievedUser.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Database_HandlesDateTimeUtcCorrectly()
    {
        // Arrange
        var scheduledTime = DateTime.UtcNow.AddHours(5);
        var flight = new Flight
        {
            FlightNumber = "UTC001",
            Airline = "UTC Airlines",
            Origin = "UTC",
            Destination = "TST",
            ScheduledDeparture = scheduledTime,
            ScheduledArrival = scheduledTime.AddHours(3),
            Gate = "UTC1",
            Terminal = "Test",
            Status = FlightStatus.Scheduled,
            Type = FlightType.Departure,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        var retrievedFlight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "UTC001");

        // Assert
        retrievedFlight.Should().NotBeNull();
        retrievedFlight!.ScheduledDeparture.Should().BeCloseTo(scheduledTime, TimeSpan.FromSeconds(1));
        retrievedFlight.ScheduledDeparture.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Database_SupportsComplexQueries()
    {
        // Arrange - Create test data with various scenarios
        await TestUtilities.SeedTestDataAsync(_factory.Services);

        // Act - Execute complex queries
        var upcomingFlights = await _context.Flights
            .Where(f => f.ScheduledDeparture > DateTime.UtcNow)
            .Where(f => f.Type == FlightType.Departure)
            .OrderBy(f => f.ScheduledDeparture)
            .Take(5)
            .ToListAsync();

        var flightsByAirline = await _context.Flights
            .GroupBy(f => f.Airline)
            .Select(g => new { Airline = g.Key, Count = g.Count() })
            .ToListAsync();

        var delayedFlights = await _context.Flights
            .Where(f => f.DelayMinutes > 0)
            .Include(f => f.Status)
            .ToListAsync();

        // Assert
        upcomingFlights.Should().NotBeEmpty();
        upcomingFlights.Should().BeInAscendingOrder(f => f.ScheduledDeparture);
        
        flightsByAirline.Should().NotBeEmpty();
        flightsByAirline.Should().OnlyContain(g => g.Count > 0);
    }
}
