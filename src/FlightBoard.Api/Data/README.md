# Data

**Entity Framework Core Data Layer**

This folder contains Entity Framework Core configuration, database context, migrations, and database seeding functionality.

## Core Files

### FlightDbContext.cs
- **Purpose**: Main database context for Entity Framework Core
- **Configuration**: Entity mappings, indexes, relationships
- **Features**: Soft delete global filters, audit trail automation
- **Provider**: SQLite with connection string configuration

### DatabaseSeeder.cs
- **Purpose**: Seed database with initial sample data
- **Data**: 60 sample flights, test user accounts (admin/operator/user)
- **Security**: Hashed passwords, realistic flight schedules
- **Usage**: Automatic seeding on application startup

## Migrations

### Purpose
- **Schema management** - Code-first database schema evolution
- **Version control** - Track database changes over time
- **Deployment** - Consistent database updates across environments

### Migration Files
- **InitialFlightSchema** - Initial database schema for flights
- **AddUserAuthentication** - User table and authentication schema
- **Future migrations** - Additional schema changes as needed

## Database Features

### Indexing
- **Performance optimization** - Strategic indexes on frequently queried columns
- **Composite indexes** - Multi-column indexes for complex queries
- **Unique constraints** - Prevent duplicate data

### Audit Trails
- **Automatic tracking** - CreatedAt, UpdatedAt timestamps
- **User tracking** - CreatedBy, UpdatedBy user identification
- **Soft deletes** - IsDeleted flag instead of physical deletion

### Configuration
- **Connection strings** - Configurable database location
- **Entity mappings** - Fluent API configuration for complex relationships
- **Query filters** - Global filters for soft deletes

## Database Location
- **Development**: `../../../Data/flightboard.db`
- **Security**: Database stored outside project directory
- **Backup**: Easy backup and restore of SQLite file
