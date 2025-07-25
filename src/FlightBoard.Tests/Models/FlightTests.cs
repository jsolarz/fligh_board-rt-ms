using Xunit;
using FlightBoard.Api.Models;

namespace FlightBoard.Tests.Models;

public class FlightTests
{
    [Fact]
    public void Flight_SetsProperties_Correctly()
    {
        // Arrange & Act
        var flight = new Flight
        {
            FlightNumber = "DL123",
            Airline = "Delta Airlines",
            Origin = "ATL",
            Destination = "SEA",
            ScheduledDeparture = new DateTime(2024, 12, 25, 10, 30, 0),
            ScheduledArrival = new DateTime(2024, 12, 25, 13, 45, 0),
            Status = FlightStatus.Scheduled
        };

        // Assert
        Assert.Equal("DL123", flight.FlightNumber);
        Assert.Equal("Delta Airlines", flight.Airline);
        Assert.Equal("ATL", flight.Origin);
        Assert.Equal("SEA", flight.Destination);
        Assert.Equal(FlightStatus.Scheduled, flight.Status);
    }

    [Theory]
    [InlineData(FlightStatus.Scheduled)]
    [InlineData(FlightStatus.Boarding)]
    [InlineData(FlightStatus.Departed)]
    [InlineData(FlightStatus.InFlight)]
    [InlineData(FlightStatus.Landed)]
    [InlineData(FlightStatus.Arrived)]
    [InlineData(FlightStatus.Delayed)]
    [InlineData(FlightStatus.Cancelled)]
    [InlineData(FlightStatus.Diverted)]
    public void Flight_AcceptsAllValidStatuses(FlightStatus status)
    {
        // Arrange & Act
        var flight = new Flight
        {
            FlightNumber = "UA789",
            Status = status
        };

        // Assert
        Assert.Equal(status, flight.Status);
    }
}
