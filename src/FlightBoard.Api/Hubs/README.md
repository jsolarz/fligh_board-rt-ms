# Hubs

**SignalR Real-time Communication Hubs**

This folder contains SignalR hubs that provide real-time, bidirectional communication between the server and connected clients. Hubs enable live updates and push notifications.

## Hubs

### FlightHub
- **Purpose**: Real-time flight information updates
- **Groups**: AllFlights, Departures, Arrivals for targeted updates
- **Events**: FlightCreated, FlightUpdated, FlightStatusChanged
- **Authentication**: JWT token validation for secure connections

## Hub Features

- **Group management** - Organize clients by interest (departures/arrivals)
- **Real-time events** - Push updates when data changes
- **Connection handling** - Automatic reconnection and error handling
- **Authentication** - JWT-based security for hub connections

## SignalR Events

### FlightCreated
- **Trigger**: New flight added to system
- **Data**: Complete flight information
- **Recipients**: All connected clients in relevant groups

### FlightUpdated  
- **Trigger**: Flight information modified
- **Data**: Updated flight details
- **Recipients**: Clients subscribed to flight updates

### FlightStatusChanged
- **Trigger**: Flight status transitions (boarding, departed, etc.)
- **Data**: Old status, new status, flight details
- **Recipients**: Targeted groups based on flight type

## Integration

- **Service integration** - Called from FlightManager via NotificationEngine
- **Frontend connection** - React apps connect using @microsoft/signalr
- **Error handling** - Graceful degradation when SignalR unavailable
- **Performance** - Efficient group-based message distribution

## Security

- **JWT authentication** - Token validation for hub connections
- **Authorization** - Role-based access to different hub methods
- **CORS** - Configured for frontend application origins
