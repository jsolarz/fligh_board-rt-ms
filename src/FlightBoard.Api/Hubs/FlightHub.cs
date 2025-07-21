// SignalR Hub for real-time flight updates
using Microsoft.AspNetCore.SignalR;

namespace FlightBoard.Api.Hubs;

public class FlightHub : Hub
{
    // Groups for different flight types
    private const string ALL_FLIGHTS_GROUP = "AllFlights";
    private const string DEPARTURES_GROUP = "Departures";
    private const string ARRIVALS_GROUP = "Arrivals";

    // Client joins specific flight groups
    public async Task JoinFlightGroups(string clientType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ALL_FLIGHTS_GROUP);

        if (clientType == "departures")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, DEPARTURES_GROUP);
        }
        else if (clientType == "arrivals")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ARRIVALS_GROUP);
        }
    }

    // Client leaves groups when disconnecting
    public async Task LeaveFlightGroups()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ALL_FLIGHTS_GROUP);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, DEPARTURES_GROUP);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ARRIVALS_GROUP);
    }

    // Override disconnect to clean up groups
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveFlightGroups();
        await base.OnDisconnectedAsync(exception);
    }
}
