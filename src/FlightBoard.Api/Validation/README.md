# Validation

**Custom Validation Attributes**

This folder contains custom validation attributes that implement business-specific validation rules beyond standard data annotations.

## Custom Validators

### FutureDateAttribute
- **Purpose**: Validates that a date/time value is in the future
- **Usage**: Applied to flight departure/arrival times
- **Business rule**: Prevents scheduling flights in the past
- **Error message**: "The {0} must be a future date and time"

## Validation Architecture

### Design Principles
- **Reusable** - Custom validators can be applied to multiple properties
- **Declarative** - Validation rules defined via attributes
- **Business-focused** - Implements specific business constraints
- **Error handling** - Clear, user-friendly error messages

### Usage Examples
```csharp
[FutureDate(ErrorMessage = "Departure time must be in the future")]
public DateTime ScheduledDeparture { get; init; }
```

### Integration
- **DTO validation** - Applied to data transfer objects
- **Model validation** - Can be used on domain entities
- **Controller validation** - Automatic validation in API endpoints
- **Client feedback** - Error messages returned to clients

## Validation Flow

1. **Client submission** - Data sent to API endpoint
2. **Model binding** - ASP.NET Core binds data to DTO
3. **Validation execution** - Custom attributes validate data
4. **Error collection** - Validation errors collected
5. **Response** - 400 Bad Request with validation errors if invalid

## Future Extensions

Additional custom validators can be added for:
- **Airport code validation** - Verify valid IATA/ICAO codes  
- **Flight number format** - Airline-specific flight number patterns
- **Time range validation** - Business hours, operational windows
- **Capacity constraints** - Aircraft capacity, gate availability
