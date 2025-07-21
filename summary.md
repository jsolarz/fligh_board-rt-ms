# Summary of Objectives

## Objective
Develop a professional, full-stack, real-time flight board management system using modern development practices. The system will feature a live-updating flight board, a comprehensive backend API, and a clean, user-friendly UI.

## Technologies
- **Backend**: ASP.NET Core Web API (C#), Entity Framework Core with SQLite, SignalR, xUnit/NUnit with Moq.
- **Frontend**: React + TypeScript, Redux Toolkit, TanStack Query (React Query), CSS/Tailwind/styled-components/Material UI.

## Core Features
1. **Real-Time Flight Board**:
   - SignalR for live updates.
   - Frontend table with columns: Flight Number, Destination, Departure Time, Gate, Status.

2. **Flight Management & Architecture**:
   - API Endpoints for CRUD operations and search.
   - Clean Architecture principles.
   - Persistent SQLite database with EF Core.
   - Server-side validation for required fields.

3. **Server-Side Status Calculation**:
   - Status logic based on server time (Boarding, Departed, Landed, Scheduled).

4. **Modern Frontend Development**:
   - TanStack Query for server state management.
   - Redux Toolkit for UI state management.
   - Features for adding, deleting, filtering, and searching flights.

5. **Testing**:
   - Unit tests for critical backend logic.
   - TDD approach for at least one feature.

## Bonus Features
- Frontend animations and optimistic updates.
- Backend structured logging and FluentValidation.
- Docker support for containerization.

## Submission Guidelines
- Clean, modular code with documentation.
- Unit test project included.
- README with setup instructions, architectural choices, and third-party libraries.
