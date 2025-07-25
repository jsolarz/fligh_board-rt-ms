# FlightBoard.Tests

**Unit Testing Project**

Comprehensive unit testing infrastructure for the FlightBoard API using xUnit, with proper mocking and in-memory database testing.

## Test Structure

### Services/
- **FlightServiceTests.cs** - Tests for flight business logic and CRUD operations
- **AuthServiceTests.cs** - Tests for authentication and user management
- **Service layer testing** - Business logic validation and edge cases

### Models/  
- **FlightTests.cs** - Tests for Flight entity validation and business rules
- **UserTests.cs** - Tests for User entity and role validation
- **Domain model testing** - Entity behavior and constraints

### BaseTestClass.cs
- **Purpose**: Shared test infrastructure and utilities
- **Features**: In-memory database setup, common test helpers
- **Mocking**: SignalR hub context mocking configuration

## Testing Framework

### xUnit
- **Test runner** - Modern .NET testing framework
- **Attributes** - `[Fact]`, `[Theory]`, `[InlineData]` for test cases
- **Assertions** - Rich assertion library for test validation

### Mocking with Moq
- **Mock objects** - Mock external dependencies for isolation
- **SignalR mocking** - Complex hub context mocking setup
- **Logger mocking** - Mock ILogger instances for service testing

### In-Memory Database
- **Entity Framework InMemory** - Fast, isolated database for testing
- **Data isolation** - Fresh database instance per test class
- **Migration testing** - Verify database schema changes

## Test Coverage

### Business Logic Testing
- **Flight status calculation** - Time-based status rules
- **CRUD operations** - Create, read, update, delete scenarios
- **Validation rules** - Business constraint enforcement
- **Error handling** - Exception scenarios and edge cases

### Current Test Results
- **Total tests**: 18 passing
- **Flight service**: CRUD operations, status calculation
- **Flight models**: Property validation, enum handling
- **Zero test failures** - All tests consistently passing

## Test Categories

### Unit Tests
- **Isolated testing** - Single component testing with mocked dependencies
- **Fast execution** - Quick feedback for development
- **Business logic focus** - Core business rule validation

### Integration Tests (Future)
- **API endpoint testing** - Full request/response cycle testing
- **Database integration** - Real database operations
- **SignalR testing** - Real-time communication testing

## Running Tests

```bash
dotnet test                    # Run all tests
dotnet test --logger:trx       # Generate test results
dotnet test --collect:"XPlat Code Coverage"  # Code coverage
```
