# Implementation Guide

## Step-by-Step Plan

### 1. Project Setup
- [ ] Initialize the backend project with ASP.NET Core Web API.
- [ ] Set up the frontend project with React and TypeScript.
- [ ] Configure the database with Entity Framework Core and SQLite.
- [ ] Add SignalR for real-time communication.

### 2. Backend Development
#### API Endpoints
- [ ] Implement `GET /api/flights` to retrieve the current list of flights.
- [ ] Implement `POST /api/flights` to add a new flight after validating the input.
- [ ] Implement `DELETE /api/flights/{id}` to remove a flight after confirmation.
- [ ] Implement `GET /api/flights/search` to retrieve flights based on search criteria.

#### Business Logic
- [ ] Add logic to calculate and update flight statuses dynamically.
- [ ] Ensure the backend supports real-time updates for all users.

### 3. Frontend Development
#### Real-Time Flight Board
- [ ] Display the flight board with the current status of flights.
- [ ] Ensure the flight board updates dynamically in real-time.

#### Flight Management
- [ ] Provide a form for users to add new flights with validation.
- [ ] Allow users to delete flights with confirmation.

#### Filtering and Searching
- [ ] Enable users to specify search criteria to find specific flights.
- [ ] Display flights matching the search criteria.

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
