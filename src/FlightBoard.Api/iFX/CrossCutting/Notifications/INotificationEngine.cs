using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.iFX.CrossCutting.Notifications;

/// <summary>
/// Interface for notification operations following iDesign Method principles
/// </summary>
public interface INotificationEngine
{
    Task NotifyFlightCreatedAsync(FlightDto flight);
    Task NotifyFlightUpdatedAsync(FlightDto flight, FlightDto? previousFlight = null);
    Task NotifyFlightStatusChangedAsync(int flightId, string oldStatus, string newStatus);
    Task NotifyFlightDeletedAsync(int flightId);
}
