# Managers

**Use Case Orchestration Layer**

This folder contains managers that orchestrate business flows by coordinating engines, data access, and cross-cutting concerns. Managers implement the public contracts defined in the Contract folder.

## Managers

### FlightManager
- **Implements**: `IFlightManager` from Contract folder
- **Purpose**: Orchestrates all flight-related use cases
- **Dependencies**: FlightEngine, FlightDataAccess, NotificationEngine
- **Operations**: CRUD, search, filtering, status updates

### AuthManager
- **Implements**: `IAuthManager` from Contract folder  
- **Purpose**: Orchestrates authentication and user management
- **Dependencies**: AuthEngine, UserDataAccess, JwtService, PasswordHashService
- **Operations**: Login, registration, token refresh, profile management

## iDesign Method Pattern

Managers follow the orchestration pattern:
- **No business logic** - Delegate to engines for business rules
- **Coordinate components** - Call engines + data access + cross-cutting services
- **Transaction boundaries** - Handle transaction scoping when needed
- **Error handling** - Translate exceptions to appropriate responses

## Architecture Flow

```
Controllers → Managers → (Engines + DataAccess + CrossCutting)
```

## Dependency Injection

Managers are registered as scoped services and injected into controllers via public contracts.
