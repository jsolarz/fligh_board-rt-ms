using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using FlightBoard.Api.Models;
using FlightBoard.Api.Services;
using FlightBoard.Api.Hubs;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Tests.Services;

public class FlightServiceTests : BaseTestClass
{
    private readonly FlightService _flightService;
    private readonly Mock<ILogger<FlightService>> _mockLogger;
    private readonly Mock<IHubContext<FlightHub>> _mockHubContext;

    public FlightServiceTests()
    {
        _mockLogger = new Mock<ILogger<FlightService>>();
        _mockHubContext = new Mock<IHubContext<FlightHub>>();
        _flightService = new FlightService(Context, _mockLogger.Object, _mockHubContext.Object);
    }

    [Fact]
    public async Task GetFlightsAsync_ReturnsEmptyList_WhenNoFlights()
    {
        // Arrange
        var searchDto = new FlightSearchDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _flightService.GetFlightsAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task CreateFlightAsync_AddsFlight_WhenValidDto()
    {
        // Arrange
        var createDto = new CreateFlightDto
        {
            FlightNumber = "AA123",
            Airline = "AA",
            Origin = "JFK",
            Destination = "LAX",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Status = "Scheduled",
            Type = "Departure"
        };

        // Act
        var result = await _flightService.CreateFlightAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AA123", result.FlightNumber);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetFlightByIdAsync_ReturnsFlight_WhenFlightExists()
    {
        // Arrange
        var createDto = new CreateFlightDto
        {
            FlightNumber = "BB456",
            Airline = "BA",
            Origin = "LHR",
            Destination = "DXB",
            ScheduledDeparture = DateTime.UtcNow.AddHours(3),
            ScheduledArrival = DateTime.UtcNow.AddHours(8),
            Status = "Scheduled",
            Type = "Departure"
        };

        var createdFlight = await _flightService.CreateFlightAsync(createDto);

        // Act
        var result = await _flightService.GetFlightByIdAsync(createdFlight.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BB456", result.FlightNumber);
    }

    [Fact]
    public async Task GetFlightByIdAsync_ReturnsNull_WhenFlightNotExists()
    {
        // Act
        var result = await _flightService.GetFlightByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CalculateFlightStatus_ReturnsScheduled_WhenMoreThan30MinutesBeforeDeparture()
    {
        // Arrange
        var departureTime = DateTime.UtcNow.AddMinutes(45);

        // Act
        var result = _flightService.CalculateFlightStatus(departureTime);

        // Assert
        Assert.Equal(FlightStatus.Scheduled, result);
    }

    [Fact]
    public void CalculateFlightStatus_ReturnsBoarding_WhenWithin30MinutesOfDeparture()
    {
        // Arrange
        var departureTime = DateTime.UtcNow.AddMinutes(15);

        // Act
        var result = _flightService.CalculateFlightStatus(departureTime);

        // Assert
        Assert.Equal(FlightStatus.Boarding, result);
    }

    [Fact]
    public void CalculateFlightStatus_ReturnsDeparted_WhenAfterDepartureTimeButWithin60Minutes()
    {
        // Arrange
        var departureTime = DateTime.UtcNow.AddMinutes(-30);
        var actualDeparture = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = _flightService.CalculateFlightStatus(departureTime, actualDeparture);

        // Assert
        Assert.Equal(FlightStatus.Departed, result);
    }
}
