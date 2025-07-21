**Objective**

Develop a professional, full-stack, real-time flight board management system. The goal is to build a robust and well-architected application using modern development practices.

The system must feature a live-updating flight board, a comprehensive backend API for flight management, and a clean, user-friendly UI. The emphasis is on code quality, architectural principles, and test-driven development.

**Technologies**

* **Back-end:** ASP.NET Core Web API (C\#)

  * **Database:** Entity Framework Core with SQLite

  * **Real-Time:** SignalR

  * **Testing:** xUnit or NUnit with Moq

* **Front-end:** React \+ TypeScript

  * **State Management:** Redux Toolkit & TanStack Query (React Query)

  * **Styling:** Your choice of CSS/Tailwind / styled-components / Material UI

**Core Features**

**1\. Real-Time Flight Board (Frontend & Backend)**

* **Live Updates via SignalR:** The backend **must** use a SignalR hub to broadcast all flight changes (creations, deletions) to connected clients in real-time. The frontend must not use polling.

* **Flight Display:** The frontend will display flights in a table with the following columns: Flight Number , Destination , Departure Time , Gate , and Status.

**2\. Flight Management & Architecture (Backend)**

* **Required API Endpoints:**

  * GET /api/flights \- Returns all current flights. The status for each flight must be calculated on the server.

  * POST /api/flights \- Adds a new flight, performing validation before saving.

  * DELETE /api/flights/{id} \- Deletes a flight by its ID.

  * GET /api/flights/search?status={status}\&destination={destination} \- Returns flights filtered by status and/or destination. Query parameters are optional and can be combined.

* **Clean Architecture & Data Storage:**

  * The project **must** be structured using Clean Architecture principles (e.g., Domain, Application, Infrastructure, API layers).

  * Flight data **must** be stored in a persistent SQLite database using EF Core. The in-memory option is not permitted.

* **Server-Side Validation:**

  * All fields are required : Flight Number (must be unique), Destination  Departure Time (must be in the future), and Gate

  * Return appropriate HTTP error codes and descriptive messages for any validation failures.

**3\. Server-Side Status Calculation (Backend)**

* The flight status **must** be calculated on the server and included in the API response. This ensures a single source of truth.

* Implement the following logic based on the current server time:

  * **Boarding:** From 30 minutes before departure until the departure time.

  * **Departed:** From the departure time until 60 minutes after.

  * **Landed:** More than 60 minutes after departure time.

  * **Scheduled:** More than 30 minutes before departure time.

**4\. Modern Frontend Development (Frontend)**

* **Server State Management:** Use **TanStack Query (React Query)** for all interactions with the backend API, including initial data fetching and mutations for adding/deleting flights. The local cache should be intelligently updated after successful mutations.

* **UI State Management:** Use **Redux Toolkit** to manage client-side state, such as filter values or form input.

* **Add & Delete Flights:**

  * Create a form to add a new flight, with fields for Flight Number, Destination, Gate, and Departure Time.

  * Include client-side validation to provide immediate user feedback.

  * Each flight row in the table must have a delete button that triggers the API call.

* **Filter & Search Flights:**

  * The UI must include inputs or dropdowns to filter flights by status and/or destination.

  * A "Search" button should trigger a new API request with the selected filters.

  * Include a "Clear Filters" button to reset the search and show all flights.

**5\. Testing (TDD)**

* Unit tests are a **required** part of this task.

* Critical business logic in the backend's Application layer (e.g., flight validation, status calculation)  **must** be covered by unit tests using xUnit or NUnit with Moq package for mocking.

* You should be prepared to discuss how a TDD approach was used for at least one feature.

**Bonus Features (Optional, but Encouraged)**

* **Frontend Polish:**

  * Animate new rows fading or sliding in when they are added via SignalR.

  * Animate the status field when it changes (e.g., a background color transition).

  * Implement an optimistic update on the frontend when deleting a flight.

* **Backend Extras:**

  * Implement structured logging for actions like adding or deleting flights.

  * Use a library like FluentValidation for server-side validation rules.

  * Provide a Dockerfiles and Docker-Compose file to containerize the project.

**Submission Guidelines**

* Provide clean, well-documented, and modular code with full TypeScript and C\# typings.

* The submission must include the unit test project.

* Include a README.md file wit:

  * Clear setup and run instructions for both the backend and frontend.

  * A brief explanation of your architectural choices.

  * A list of any third-party libraries used.

  * A short screen recording of the running application (optional).

