using Microsoft.AspNetCore.SignalR;
using FlightBoard.Api.Hubs;
using FlightBoard.Api.Core.DTOs;

namespace FlightBoard.Api.CrossCutting.Notifications;

/// <summary>
/// SignalR-based notification engine following iDesign Method principles
/// Handles all real-time notifications as a cross-cutting concern
/// </summary>
public class SignalRNotificationEngine : INotificationEngine
{
    private readonly IHubContext<FlightHub> _hubContext;
    private readonly ILogger<SignalRNotificationEngine> _logger;

    public SignalRNotificationEngine(IHubContext<FlightHub> hubContext, ILogger<SignalRNotificationEngine> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Notify all clients about a new flight
    /// </summary>
    public async Task NotifyFlightCreatedAsync(FlightDto flight)
    {
        try
        {
            _logger.LogInformation("Broadcasting flight created notification for {FlightNumber}", flight.FlightNumber);

            // Broadcast to all connected clients
            await _hubContext.Clients.All.SendAsync("FlightCreated", flight);

            // Broadcast to specific groups based on flight type
            var groupName = flight.Type == "Departure" ? "Departures" : "Arrivals";
            await _hubContext.Clients.Group(groupName).SendAsync("FlightAdded", flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting flight created notification for {FlightNumber}", flight.FlightNumber);
        }
    }

    /// <summary>
    /// Notify all clients about flight updates
    /// </summary>
    public async Task NotifyFlightUpdatedAsync(FlightDto flight, FlightDto? previousFlight = null)
    {
        try
        {
            _logger.LogInformation("Broadcasting flight updated notification for {FlightNumber}", flight.FlightNumber);

            // Broadcast to all connected clients
            await _hubContext.Clients.All.SendAsync("FlightUpdated", flight);

            // If status changed, send specific status change notification
            if (previousFlight != null && previousFlight.Status != flight.Status)
            {
                await NotifyFlightStatusChangedAsync(flight.Id, previousFlight.Status, flight.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting flight updated notification for {FlightNumber}", flight.FlightNumber);
        }
    }

    /// <summary>
    /// Notify all clients about status changes
    /// </summary>
    public async Task NotifyFlightStatusChangedAsync(int flightId, string oldStatus, string newStatus)
    {
        try
        {
            _logger.LogInformation("Broadcasting status change notification for flight {FlightId}: {OldStatus} -> {NewStatus}",
                flightId, oldStatus, newStatus);

            var statusChangeData = new
            {
                FlightId = flightId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("FlightStatusChanged", statusChangeData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting status change notification for flight {FlightId}", flightId);
        }
    }

    /// <summary>
    /// Notify all clients about flight deletion
    /// </summary>
    public async Task NotifyFlightDeletedAsync(int flightId)
    {
        try
        {
            _logger.LogInformation("Broadcasting flight deleted notification for {FlightId}", flightId);

            await _hubContext.Clients.All.SendAsync("FlightDeleted", flightId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting flight deleted notification for {FlightId}", flightId);
        }
    }
}
