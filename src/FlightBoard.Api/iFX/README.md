# iFX Framework

The **iFX** (infrastructure Framework eXtensions) provides a structured approach to organizing cross-cutting concerns, utilities, and shared functionality in the Flight Board application following enterprise patterns.

## Architecture Overview

The iFX framework is organized into logical layers that support the main application architecture:

```
iFX/
├── CrossCutting/           # Cross-cutting concerns available to all layers
│   ├── Notifications/      # Real-time notification services
│   └── Logging/           # Enhanced logging capabilities
└── Utilities/             # Shared utilities and helpers
    ├── Extensions/        # Extension methods for domain objects
    ├── Helpers/          # Business logic helpers and calculations  
    └── Mapping/          # Object mapping utilities
```

## Components

### CrossCutting Concerns

#### Notifications
- **INotificationEngine / SignalRNotificationEngine**: Handles all real-time notifications via SignalR
- Provides flight creation, update, status change, and deletion notifications
- Supports group-based broadcasting (Departures, Arrivals)

#### Logging
- **FlightBoardLogger**: Enhanced structured logging with contextual information
- Provides specialized logging methods for operations, API requests, validation, notifications, and database operations

### Utilities

#### Extensions
- **DateTimeExtensions**: Time calculations for flight operations
- **FlightExtensions**: Domain-specific flight behavior extensions

#### Helpers
- **FlightStatusHelper**: Flight status calculations and business rules
- **PaginationHelper**: Pagination logic and calculations

#### Mapping
- **IFlightMappingUtility / FlightMappingUtility**: Entity-to-DTO mapping operations

## Usage Patterns

### Service Registration

```csharp
// Register all iFX services at once
builder.Services.AddiFXServices();
```

### Dependency Injection

```csharp
public class FlightManager : IFlightManager
{
    private readonly INotificationEngine _notificationEngine;
    private readonly IFlightMappingUtility _mappingUtility;
    
    public FlightManager(
        INotificationEngine notificationEngine,
        IFlightMappingUtility mappingUtility)
    {
        _notificationEngine = notificationEngine;
        _mappingUtility = mappingUtility;
    }
}
```

### Extension Method Usage

```csharp
// DateTime extensions
if (flight.ScheduledDeparture.IsDepartingSoon())
{
    // Handle boarding logic
}

// Flight extensions
if (flight.IsActive())
{
    // Handle active flight logic
}

// Enhanced logging
logger.LogFlightOperation("CreateFlight", flight.FlightNumber, additionalData);
```

## Design Principles

1. **Separation of Concerns**: Each component has a single, well-defined responsibility
2. **Dependency Inversion**: Components depend on abstractions, not concretions
3. **Cross-cutting Support**: Concerns like logging and notifications are available to all layers
4. **Maintainability**: Clear folder structure and naming conventions
5. **Testability**: All components are interface-based for easy mocking
6. **Enterprise Patterns**: Follows established patterns for scalable applications

## Benefits

- **Centralized Infrastructure**: All cross-cutting concerns in one place
- **Consistent Patterns**: Standardized approach to common operations
- **Reduced Duplication**: Shared utilities prevent code repetition
- **Enhanced Maintainability**: Clear separation makes changes easier
- **Improved Testability**: Interface-based design supports unit testing
- **Scalability**: Framework can easily accommodate new utilities and concerns

## Extending iFX

To add new components to iFX:

1. Create the appropriate folder structure under `iFX/`
2. Implement interfaces following dependency inversion principle  
3. Add registration to `iFXServiceCollectionExtensions`
4. Update this README with usage examples

---

*iFX Framework - Infrastructure Extensions for the Flight Board Application*
