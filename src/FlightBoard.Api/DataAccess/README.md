# DataAccess

**Data Persistence Layer**

This folder contains data access components that handle all database operations using Entity Framework Core. Following iDesign Method principles, data access is completely separated from business logic.

## Structure

### Flight/
- **IFlightDataAccess.cs** - Data access interface for flights
- **FlightDataAccess.cs** - EF Core implementation for flight operations

### User/  
- **IUserDataAccess.cs** - Data access interface for users
- **UserDataAccess.cs** - EF Core implementation for user operations

## Responsibilities

- **CRUD operations** - Create, Read, Update, Delete entities
- **Query optimization** - Efficient database queries with proper indexing
- **Data mapping** - Convert between entities and DTOs
- **Transaction handling** - Database transaction management

## Architecture Pattern

- **Repository pattern** - Abstracted data access through interfaces
- **Dependency injection** - Interfaces injected into managers
- **Entity Framework Core** - ORM for database operations
- **Async operations** - All database calls are asynchronous

## Database Features

- **SQLite provider** - Lightweight database for development and deployment
- **Migrations** - Code-first database schema management
- **Indexing** - Optimized indexes for query performance
- **Soft deletes** - Logical deletion with IsDeleted flag
- **Audit trails** - CreatedAt, UpdatedAt, CreatedBy, UpdatedBy tracking

## iDesign Method Compliance

DataAccess components are called by managers alongside engines to implement complete use cases while maintaining clean separation of concerns.
