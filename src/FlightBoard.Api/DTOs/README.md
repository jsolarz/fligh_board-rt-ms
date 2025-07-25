# DTOs (Data Transfer Objects)

**API Data Transfer Objects**

This folder contains DTOs (Data Transfer Objects) that define the structure of data transferred between the API and clients. DTOs are implemented as modern C# records with comprehensive validation.

## DTO Files

### FlightDtos.cs
- **Purpose**: Flight-related data transfer objects
- **DTOs**: FlightDto, CreateFlightDto, UpdateFlightDto, FlightSearchDto
- **Validation**: Comprehensive validation attributes, custom validators
- **Features**: Required fields, future date validation, business constraints

### AuthDtos.cs  
- **Purpose**: Authentication and user management DTOs
- **DTOs**: LoginDto, RegisterDto, AuthResponseDto, UserProfileDto
- **Validation**: Email format, password strength, username constraints
- **Security**: No sensitive data exposure (password hashes excluded)

## DTO Principles

- **Modern C# records** - Immutable by default with `init` properties
- **Required properties** - `required` keyword for mandatory fields
- **Validation attributes** - Data annotations for comprehensive validation
- **Business rules** - Custom validation attributes (e.g., FutureDateAttribute)

## Validation Features

- **Required fields** - Compile-time and runtime validation
- **Format validation** - Email, phone, date formats
- **Length constraints** - String length limitations
- **Custom validators** - Business-specific validation rules
- **Error messages** - Clear, user-friendly validation messages

## Mapping

DTOs are mapped to/from domain entities using:
- **Pure C# mapping** - No external mapping libraries
- **Mapping utilities** - Centralized conversion logic in iFX framework
- **Type safety** - Compile-time checking with strong typing

## API Integration

DTOs serve as the contract between:
- **Client applications** - Frontend TypeScript interfaces match DTOs
- **API endpoints** - Controllers accept/return DTOs
- **Documentation** - OpenAPI/Swagger generation from DTO attributes
