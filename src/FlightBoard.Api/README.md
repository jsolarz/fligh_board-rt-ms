# FlightBoard.Api

**Main .NET 9 Web API Project**

This is the backend API for the Flight Board Real-time Management System, implementing enterprise-grade architecture following the iDesign Method with JWT authentication and SignalR real-time updates.

## Architecture

The project follows iDesign Method principles with clear separation of concerns:

- **Controllers/** - API endpoints and request/response handling
- **Contract/** - Public manager interfaces (external API contracts)
- **Managers/** - Use case orchestration layer
- **Engines/** - Pure business logic with no external dependencies
- **DataAccess/** - Data persistence layer with EF Core
- **iFX/** - Infrastructure framework for cross-cutting concerns
- **Models/** - Domain entities and data models
- **DTOs/** - Data transfer objects for API communication
- **Hubs/** - SignalR hubs for real-time communication

## Key Features

- ✅ JWT Authentication & Authorization with role-based access control
- ✅ Real-time updates via SignalR
- ✅ Comprehensive flight management CRUD operations
- ✅ Advanced search and filtering capabilities
- ✅ SQLite database with Entity Framework Core
- ✅ Modern C# 9 features (records, required properties)
- ✅ Comprehensive validation and error handling
- ✅ Production-ready logging and health checks

## Configuration

- **appsettings.json** - Production configuration
- **appsettings.Development.json** - Development overrides
- **Program.cs** - Application startup and service configuration

## Database

- **Provider**: SQLite
- **Location**: `../../../Data/flightboard.db`
- **Migrations**: `Migrations/` folder
- **Seeding**: Automatic seed data for flights and users
