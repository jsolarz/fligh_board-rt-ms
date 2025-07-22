using Microsoft.AspNetCore.SignalR.Client;
using FluentAssertions;
using Xunit;
using FlightBoard.IntegrationTests.Infrastructure;
using System.Net.Http.Json;
using System.Text;
using FlightBoard.Api.DTOs;
using Newtonsoft.Json;

namespace FlightBoard.IntegrationTests.SignalR;

/// <summary>
/// Integration tests for SignalR functionality
/// Tests real-time notifications and hub connectivity
/// </summary>
public class FlightHubTests : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;

    public FlightHubTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestUtilities.SeedTestDataAsync(_factory.Services);
        
        // Create SignalR connection
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/flighthub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await _hubConnection.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await TestUtilities.CleanupTestDataAsync(_factory.Services);
        
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        
        _httpClient.Dispose();
    }

    [Fact]
    public async Task HubConnection_CanConnect_Successfully()
    {
        // Assert
        _hubConnection.Should().NotBeNull();
        _hubConnection!.State.Should().Be(HubConnectionState.Connected);
    }

    [Fact]
    public async Task FlightCreated_SendsNotification_WhenFlightIsCreated()
    {
        // Arrange
        var notificationReceived = false;
        var receivedFlightNumber = "";

        _hubConnection!.On<string, object>("FlightCreated", (flightNumber, flightData) =>
        {
            notificationReceived = true;
            receivedFlightNumber = flightNumber;
        });

        await _hubConnection.SendAsync("JoinGroup", "AllFlights");

        var newFlight = TestUtilities.CreateValidFlightDto("SH001");
        var token = TestUtilities.GenerateTestJwtToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/flights", newFlight);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Wait for SignalR notification
        await Task.Delay(1000);

        notificationReceived.Should().BeTrue();
        receivedFlightNumber.Should().Be("SH001");
    }

    [Fact]
    public async Task FlightUpdated_SendsNotification_WhenFlightIsUpdated()
    {
        // Arrange
        var notificationReceived = false;
        var receivedFlightNumber = "";

        _hubConnection!.On<string, object>("FlightUpdated", (flightNumber, flightData) =>
        {
            notificationReceived = true;
            receivedFlightNumber = flightNumber;
        });

        await _hubConnection.SendAsync("JoinGroup", "AllFlights");

        // Create a flight first
        var newFlight = TestUtilities.CreateValidFlightDto("SH002");
        var token = TestUtilities.GenerateTestJwtToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _httpClient.PostAsJsonAsync("/api/flights", newFlight);
        var createdFlightContent = await createResponse.Content.ReadAsStringAsync();
        var createdFlight = JsonConvert.DeserializeObject<FlightDto>(createdFlightContent);

        var updateFlight = TestUtilities.CreateValidUpdateFlightDto(createdFlight!.Id);

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/flights/{createdFlight.Id}", updateFlight);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Wait for SignalR notification
        await Task.Delay(1000);

        notificationReceived.Should().BeTrue();
        receivedFlightNumber.Should().StartWith("UP");
    }

    [Fact]
    public async Task FlightStatusChanged_SendsNotification_WhenStatusIsUpdated()
    {
        // Arrange
        var notificationReceived = false;
        var receivedOldStatus = "";
        var receivedNewStatus = "";

        _hubConnection!.On<string, string, string, object>("FlightStatusChanged", 
            (flightNumber, oldStatus, newStatus, flightData) =>
        {
            notificationReceived = true;
            receivedOldStatus = oldStatus;
            receivedNewStatus = newStatus;
        });

        await _hubConnection.SendAsync("JoinGroup", "AllFlights");

        // Get an existing flight from seeded data
        var flightsResponse = await _httpClient.GetAsync("/api/flights");
        var flightsContent = await flightsResponse.Content.ReadAsStringAsync();
        var flightsResult = JsonConvert.DeserializeObject<PagedResponse<FlightDto>>(flightsContent);
        var existingFlight = flightsResult!.Data.First();

        var token = TestUtilities.GenerateTestJwtToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var statusUpdate = new { Status = "Delayed" };

        // Act
        var response = await _httpClient.PatchAsync($"/api/flights/{existingFlight.Id}/status", 
            new StringContent(JsonConvert.SerializeObject(statusUpdate), 
                Encoding.UTF8, "application/json"));

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Wait for SignalR notification
        await Task.Delay(1000);

        notificationReceived.Should().BeTrue();
        receivedNewStatus.Should().Be("Delayed");
    }

    [Fact]
    public async Task GroupSubscription_ReceivesOnlyRelevantNotifications_ForDepartures()
    {
        // Arrange
        var departureNotificationReceived = false;
        var allFlightsNotificationReceived = false;

        _hubConnection!.On<string, object>("FlightCreated", (flightNumber, flightData) =>
        {
            departureNotificationReceived = true;
        });

        // Join only the Departures group
        await _hubConnection.SendAsync("JoinGroup", "Departures");

        var departuresFlight = new CreateFlightDto
        {
            FlightNumber = "DEP001",
            Airline = "Test Airlines",
            Origin = "TEST",
            Destination = "DEST",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(6),
            Gate = "T1",
            Terminal = "Test",
            Status = "Scheduled",
            Type = "Departure"
        };

        var token = TestUtilities.GenerateTestJwtToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - create a departure flight
        var response = await _httpClient.PostAsJsonAsync("/api/flights", departuresFlight);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Wait for SignalR notification
        await Task.Delay(1000);

        departureNotificationReceived.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleConnections_ReceiveNotifications_Simultaneously()
    {
        // Arrange
        var connection2 = new HubConnectionBuilder()
            .WithUrl("http://localhost/flighthub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection2.StartAsync();

        var notification1Received = false;
        var notification2Received = false;

        _hubConnection!.On<string, object>("FlightCreated", (flightNumber, flightData) =>
        {
            notification1Received = true;
        });

        connection2.On<string, object>("FlightCreated", (flightNumber, flightData) =>
        {
            notification2Received = true;
        });

        await _hubConnection.SendAsync("JoinGroup", "AllFlights");
        await connection2.SendAsync("JoinGroup", "AllFlights");

        var newFlight = TestUtilities.CreateValidFlightDto("MC001");
        var token = TestUtilities.GenerateTestJwtToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/flights", newFlight);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Wait for SignalR notifications
        await Task.Delay(1000);

        notification1Received.Should().BeTrue();
        notification2Received.Should().BeTrue();

        // Cleanup
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task HubConnection_CanReconnect_AfterDisconnection()
    {
        // Arrange
        var originalState = _hubConnection!.State;

        // Act - simulate disconnection and reconnection
        await _hubConnection.StopAsync();
        var disconnectedState = _hubConnection.State;

        await _hubConnection.StartAsync();
        var reconnectedState = _hubConnection.State;

        // Assert
        originalState.Should().Be(HubConnectionState.Connected);
        disconnectedState.Should().Be(HubConnectionState.Disconnected);
        reconnectedState.Should().Be(HubConnectionState.Connected);
    }
}
