using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightBoard.Api.Services;

/// <summary>
/// Service for seeding the database with sample flight data
/// </summary>
public class DatabaseSeeder
{
    private readonly FlightDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(FlightDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed the database with sample flight data if no flights exist
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if database has any flights
            if (await _context.Flights.AnyAsync())
            {
                _logger.LogInformation("Database already contains flight data, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding database with sample flight data");

            var sampleFlights = GenerateSampleFlights();
            await _context.Flights.AddRangeAsync(sampleFlights);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} flights into database", sampleFlights.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    /// <summary>
    /// Generate sample flight data for testing and demonstration
    /// </summary>
    private List<Flight> GenerateSampleFlights()
    {
        var baseDate = DateTime.UtcNow.Date;
        var flights = new List<Flight>();

        // Sample airlines and airports
        var airlines = new[] { "AA", "DL", "UA", "BA", "LH", "AF", "KL", "EK" };
        var airports = new[] { "JFK", "LAX", "LHR", "CDG", "FRA", "AMS", "DXB", "ORD", "ATL", "DEN" };
        var aircraftTypes = new[] { "Boeing 737", "Airbus A320", "Boeing 777", "Airbus A350", "Boeing 787", "Airbus A330" };

        var random = new Random();

        // Generate flights for today and tomorrow
        for (int day = 0; day < 2; day++)
        {
            var currentDate = baseDate.AddDays(day);

            // Generate departures
            for (int i = 0; i < 15; i++)
            {
                var airline = airlines[random.Next(airlines.Length)];
                var origin = airports[random.Next(airports.Length)];
                var destination = airports.Where(a => a != origin).ToArray()[random.Next(airports.Length - 1)];

                var scheduledDeparture = currentDate.AddHours(6 + i * 1.2).AddMinutes(random.Next(-30, 30));
                var flightDuration = TimeSpan.FromHours(random.Next(2, 12)); // 2-12 hour flights
                var scheduledArrival = scheduledDeparture.Add(flightDuration);

                var delayMinutes = GenerateRandomDelay(random);
                var status = GenerateFlightStatus(scheduledDeparture, delayMinutes, random);

                flights.Add(new Flight
                {
                    FlightNumber = $"{airline}{random.Next(100, 9999)}",
                    Airline = airline,
                    Origin = origin,
                    Destination = destination,
                    ScheduledDeparture = scheduledDeparture,
                    ActualDeparture = status >= FlightStatus.Departed ? scheduledDeparture.AddMinutes(delayMinutes) : null,
                    ScheduledArrival = scheduledArrival,
                    ActualArrival = status == FlightStatus.Arrived ? scheduledArrival.AddMinutes(delayMinutes) : null,
                    Status = status,
                    Gate = GenerateGate(random),
                    Terminal = $"Terminal {random.Next(1, 4)}",
                    AircraftType = aircraftTypes[random.Next(aircraftTypes.Length)],
                    Remarks = GenerateRemarks(status, delayMinutes, random),
                    DelayMinutes = delayMinutes,
                    Type = FlightType.Departure,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 120))
                });
            }

            // Generate arrivals
            for (int i = 0; i < 15; i++)
            {
                var airline = airlines[random.Next(airlines.Length)];
                var origin = airports[random.Next(airports.Length)];
                var destination = airports.Where(a => a != origin).ToArray()[random.Next(airports.Length - 1)];

                var scheduledArrival = currentDate.AddHours(6 + i * 1.2).AddMinutes(random.Next(-30, 30));
                var flightDuration = TimeSpan.FromHours(random.Next(2, 12));
                var scheduledDeparture = scheduledArrival.Subtract(flightDuration);

                var delayMinutes = GenerateRandomDelay(random);
                var status = GenerateFlightStatus(scheduledDeparture, delayMinutes, random, isArrival: true);

                flights.Add(new Flight
                {
                    FlightNumber = $"{airline}{random.Next(100, 9999)}",
                    Airline = airline,
                    Origin = origin,
                    Destination = destination,
                    ScheduledDeparture = scheduledDeparture,
                    ActualDeparture = status >= FlightStatus.Departed ? scheduledDeparture.AddMinutes(delayMinutes) : null,
                    ScheduledArrival = scheduledArrival,
                    ActualArrival = status == FlightStatus.Arrived ? scheduledArrival.AddMinutes(delayMinutes) : null,
                    Status = status,
                    Gate = GenerateGate(random),
                    Terminal = $"Terminal {random.Next(1, 4)}",
                    AircraftType = aircraftTypes[random.Next(aircraftTypes.Length)],
                    Remarks = GenerateRemarks(status, delayMinutes, random),
                    DelayMinutes = delayMinutes,
                    Type = FlightType.Arrival,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 120))
                });
            }
        }

        return flights;
    }

    private int GenerateRandomDelay(Random random)
    {
        // 70% on time, 20% delayed, 10% early
        var delayType = random.Next(100);

        if (delayType < 70) return 0; // On time
        if (delayType < 90) return random.Next(5, 180); // Delayed 5-180 minutes
        return -random.Next(5, 30); // Early 5-30 minutes
    }

    private FlightStatus GenerateFlightStatus(DateTime scheduledTime, int delayMinutes, Random random, bool isArrival = false)
    {
        var now = DateTime.UtcNow;
        var estimatedTime = scheduledTime.AddMinutes(delayMinutes);

        // 5% chance of cancellation
        if (random.Next(100) < 5)
            return FlightStatus.Cancelled;

        // 2% chance of diversion for arrivals
        if (isArrival && random.Next(100) < 2)
            return FlightStatus.Diverted;

        if (estimatedTime > now.AddHours(2))
            return FlightStatus.Scheduled;

        if (estimatedTime > now.AddMinutes(30))
            return delayMinutes > 15 ? FlightStatus.Delayed : FlightStatus.Scheduled;

        if (estimatedTime > now.AddMinutes(-30))
            return random.Next(2) == 0 ? FlightStatus.Boarding : FlightStatus.Scheduled;

        if (estimatedTime > now.AddMinutes(-60))
            return FlightStatus.Departed;

        if (isArrival)
        {
            if (estimatedTime <= now.AddMinutes(-30))
                return FlightStatus.Arrived;
            return FlightStatus.Landed;
        }

        return FlightStatus.InFlight;
    }

    private string GenerateGate(Random random)
    {
        var gates = new[] { "A", "B", "C", "D", "E" };
        var gatePrefix = gates[random.Next(gates.Length)];
        var gateNumber = random.Next(1, 50);
        return $"{gatePrefix}{gateNumber}";
    }

    private string? GenerateRemarks(FlightStatus status, int delayMinutes, Random random)
    {
        var remarks = new List<string>();

        switch (status)
        {
            case FlightStatus.Delayed:
                var delayReasons = new[]
                {
                    "Weather conditions",
                    "Air traffic control delay",
                    "Aircraft maintenance",
                    "Crew scheduling",
                    "Ground operations delay"
                };
                remarks.Add(delayReasons[random.Next(delayReasons.Length)]);
                break;

            case FlightStatus.Cancelled:
                var cancelReasons = new[]
                {
                    "Severe weather conditions",
                    "Aircraft maintenance issue",
                    "Crew unavailability",
                    "Operational requirements"
                };
                remarks.Add(cancelReasons[random.Next(cancelReasons.Length)]);
                break;

            case FlightStatus.Diverted:
                remarks.Add("Diverted due to weather at destination");
                break;

            case FlightStatus.Boarding:
                remarks.Add("Now boarding all passengers");
                break;
        }

        return remarks.Any() ? string.Join(". ", remarks) : null;
    }
}
