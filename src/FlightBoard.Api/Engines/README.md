# Engines

**Pure Business Logic Layer**

This folder contains engines that implement pure business logic with no external dependencies. Engines are the core of the application's business rules and calculations.

## Engines

### FlightEngine
- **Purpose**: Flight business rules and calculations
- **Logic**: Status calculation, validation rules, business constraints
- **Dependencies**: None (pure business logic)
- **Methods**: Status calculation, flight validation, business rule enforcement

### AuthEngine  
- **Purpose**: Authentication business logic
- **Logic**: Password strength validation, account policies, security rules
- **Dependencies**: None (pure business logic)
- **Methods**: Credential validation, password policy enforcement

## iDesign Method Principles

Engines follow strict guidelines:
- **No external dependencies** - Pure business logic only
- **Stateless** - No instance state, only method parameters
- **Testable** - Easy to unit test without mocking
- **Focused** - Single responsibility for business domain

## Business Logic Examples

- **Flight status calculation** based on departure/arrival times
- **Password strength validation** according to security policies  
- **Flight validation rules** for schedules and constraints
- **Business constraints** like minimum departure times

## Architecture Position

```
Managers → Engines (+ DataAccess + CrossCutting)
```

Engines are called by managers alongside data access and cross-cutting services to implement complete use cases.
