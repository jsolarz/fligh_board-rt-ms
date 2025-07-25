# Contract

**Public Manager Interfaces (External API Contracts)**

This folder contains public contracts that define the external API for the application. Following iDesign Method principles, only manager interfaces are exposed as public contracts.

## Structure

### Flight/
- **IFlightManager.cs** - Public contract for flight operations
- Defines all flight-related use cases available to external consumers
- Abstracts internal implementation details

## iDesign Method Compliance

- **Public interfaces only** - Only manager contracts are public
- **Volatility encapsulation** - Hides internal component changes
- **Dependency inversion** - External consumers depend on abstractions
- **Contract stability** - Public APIs remain stable while internals can evolve

## Usage

Controllers and external consumers interact only through these contracts:
```csharp
public FlightsController(IFlightManager flightManager)
{
    _flightManager = flightManager;
}
```

## Namespace Convention

- **Pattern**: `FlightBoard.Api.Contract.<Domain>`
- **Examples**: 
  - `FlightBoard.Api.Contract.Flight`
  - `FlightBoard.Api.Contract.Auth` (future)

This ensures clear separation between public contracts and internal implementation.
