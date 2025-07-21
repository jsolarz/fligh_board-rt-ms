# Implementation Guide

## Step-by-Step Plan

### 1. Project Setup
- [ ] Initialize the backend project with ASP.NET Core Web API.
- [ ] Set up the frontend project with React and TypeScript.
- [ ] Configure the database with Entity Framework Core and SQLite.
- [ ] Add SignalR for real-time communication.

### 2. Backend Development
#### API Endpoints
- [ ] Implement `GET /api/flights`.
- [ ] Implement `POST /api/flights` with validation.
- [ ] Implement `DELETE /api/flights/{id}`.
- [ ] Implement `GET /api/flights/search` with query parameters.

#### Business Logic
- [ ] Add server-side status calculation logic.
- [ ] Implement Clean Architecture layers (Domain, Application, Infrastructure, API).

### 3. Frontend Development
#### Real-Time Flight Board
- [ ] Create a table to display flights.
- [ ] Integrate SignalR for live updates.

#### Flight Management
- [ ] Add a form to create new flights with validation.
- [ ] Add delete functionality for flights.

#### Filtering and Searching
- [ ] Add UI for filtering flights by status and destination.
- [ ] Implement search functionality.

### 4. State Management
- [ ] Use TanStack Query for server state management.
- [ ] Use Redux Toolkit for UI state management.

### 5. Testing
- [ ] Write unit tests for backend logic using xUnit/NUnit and Moq.
- [ ] Test frontend components and API interactions.

### 6. Bonus Features (Optional)
- [ ] Add frontend animations for new rows and status changes.
- [ ] Implement structured logging in the backend.
- [ ] Add Docker support for containerization.

### 7. Documentation
- [ ] Write a README with setup instructions, architectural choices, and third-party libraries.
- [ ] Record a short screen capture of the running application (optional).
