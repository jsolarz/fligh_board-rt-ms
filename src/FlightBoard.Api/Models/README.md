# Models

**Domain Entities and Data Models**

This folder contains the core domain models that represent the business entities in the application. Models inherit from BaseEntity for audit tracking and soft delete functionality.

## Entities

### Flight
- **Purpose**: Core flight entity with comprehensive flight information
- **Properties**: Flight number, airline, origin, destination, times, status, gates
- **Validation**: Required fields, business rule constraints
- **Features**: Status calculation, comprehensive indexing

### User
- **Purpose**: User entity for authentication and authorization  
- **Properties**: Username, email, password hash, role, profile information
- **Security**: Password hash storage, role-based access control
- **Roles**: User (read-only), Operator (flight management), Admin (full access)

### BaseEntity
- **Purpose**: Base class providing common entity functionality
- **Audit fields**: Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **Soft delete**: IsDeleted flag for logical deletion
- **Inheritance**: All domain entities inherit from BaseEntity

### HealthCheckResult
- **Purpose**: Health check data model for API monitoring
- **Properties**: Status, connectivity, database metrics
- **Usage**: Health endpoints for application monitoring

## Entity Features

- **Audit trails** - Automatic tracking of creation and modification
- **Soft deletes** - Logical deletion preserves data integrity
- **Validation attributes** - Data annotation validation
- **EF Core mapping** - Entity Framework configuration and relationships

## Business Rules

Models encapsulate business rules through:
- **Property validation** - Required fields, data formats, constraints
- **Enums** - FlightStatus, UserRole for type safety
- **Computed properties** - Derived values and convenience accessors
