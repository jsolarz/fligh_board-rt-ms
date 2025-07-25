# Controllers

**API Endpoints and Request/Response Handling**

This folder contains all API controllers that handle HTTP requests and responses. Controllers follow REST principles and delegate business logic to managers.

## Controllers

### FlightsController
- **Endpoint**: `/api/flights`
- **Purpose**: Comprehensive flight management API
- **Operations**: CRUD operations, search, filtering, pagination
- **Authentication**: JWT required for POST/PUT/DELETE operations

### AuthController  
- **Endpoint**: `/api/auth`
- **Purpose**: Authentication and user management
- **Operations**: Login, register, refresh tokens, profile management
- **Security**: Password hashing, JWT token generation

### HealthController
- **Endpoint**: `/health`
- **Purpose**: Application health monitoring
- **Operations**: Basic health check, detailed system status
- **Monitoring**: Database connectivity, memory usage, system metrics

## Architecture Pattern

Controllers follow the iDesign Method pattern:
- **Thin controllers** - Minimal logic, delegate to managers
- **Dependency injection** - All dependencies injected via constructor
- **Error handling** - Consistent error responses with proper HTTP status codes
- **Validation** - Model validation with comprehensive error messages

## Security

- **JWT Authentication** - Required for protected endpoints
- **Role-based authorization** - Admin, Operator, User roles
- **CORS** - Configured for frontend applications
- **Input validation** - Comprehensive DTO validation
