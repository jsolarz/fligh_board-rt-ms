# Use Cases

## Identified Volatilities
1. **Real-Time Updates**: Changes in how real-time updates are handled (e.g., SignalR implementation).
2. **Flight Data Storage**: Potential changes in database technology or schema.
3. **Frontend Framework**: Updates to React or state management libraries.
4. **API Contracts**: Modifications to API endpoints or data structures.
5. **Validation Rules**: Changes in business rules for flight validation.
6. **Authentication & Authorization**: Changes in user roles and access control mechanisms.
7. **Performance Requirements**: Changes in SLA requirements and performance metrics.
8. **Third-party Integrations**: Changes in external service APIs and data sources.
9. **Backup & Recovery**: Changes in data retention and disaster recovery policies.
10. **Monitoring & Alerting**: Changes in monitoring tools and alerting mechanisms.

## Use Cases

### Core Use Cases

#### 1. User Authentication and Authorization
**Flow:**
1. User attempts to access the system (Backoffice or Consumer Application).
2. System validates user credentials against the Users table.
3. System retrieves user roles from UserRoles table.
4. System grants appropriate access based on role permissions.

**Mermaid Diagram:**
```mermaid
graph TD
    A[User Login] --> B[Validate Credentials]
    B --> C{Valid User?}
    C -- Yes --> D[Retrieve User Roles]
    D --> E[Grant Access Based on Roles]
    C -- No --> F[Deny Access]
    E --> G{Admin Role?}
    G -- Yes --> H[Access Backoffice + Consumer]
    G -- No --> I[Access Consumer Only]
```

#### 2. Display Real-Time Flight Board
**Updated Flow:**
1. Consumer accesses the flight board application.
2. **FlightManager** orchestrates data retrieval through **FlightEngine**.
3. **FlightAccessor** fetches flight data from the database.
4. System calculates real-time flight statuses.
5. SignalR broadcasts updates to all connected clients.
6. Frontend displays updated flight information.

**Enhanced Mermaid Diagram:**
```mermaid
graph TD
    A[User accesses flight board] --> B[FlightManager orchestrates]
    B --> C[FlightEngine processes request]
    C --> D[FlightAccessor retrieves data]
    D --> E[Calculate flight statuses]
    E --> F[SignalR broadcasts updates]
    F --> G[All clients receive updates]
    G --> H[UI displays real-time data]
```

#### 3. Add a New Flight
- Specified that the **FlightManager** validates and processes the request by calling the **FlightEngine**.
- Clarified that the **FlightAccessor** handles database operations for storing the new flight.

#### Flow
1. The user provides details for a new flight.
2. The system validates the provided details.
3. If valid, the system adds the flight to the list and ensures it is visible to all users.

#### Mermaid Diagram
```mermaid
graph TD
    A[User provides flight details] --> B[System validates details]
    B --> C{Details valid?}
    C -- Yes --> D[System adds flight to the list]
    D --> E[Flight is visible to all users]
    C -- No --> F[System notifies user of errors]
```

#### 4. Delete a Flight
- Updated to reflect that the **FlightManager** confirms and processes the deletion request.
- Mentioned that the **FlightAccessor** ensures the flight is removed from the database.

#### Flow
1. The user requests to remove a flight.
2. The system confirms the request with the user.
3. If confirmed, the system removes the flight and ensures it is no longer visible to users.

#### Mermaid Diagram
```mermaid
graph TD
    A[User requests to remove flight] --> B[System confirms request]
    B --> C{User confirms?}
    C -- Yes --> D[System removes flight]
    D --> E[Flight is no longer visible to users]
    C -- No --> F[System cancels deletion]
```

#### 5. Search and Filter Flights
- Added that the **FlightManager** retrieves matching flights by delegating to the **FlightEngine** and **FlightAccessor**.

#### Flow
1. The user specifies criteria to find specific flights.
2. The system retrieves and displays flights matching the criteria.

#### Mermaid Diagram
```mermaid
graph TD
    A[User specifies search criteria] --> B[System retrieves matching flights]
    B --> C[System displays matching flights]
```

#### 6. Calculate Flight Status
- Clarified that the **FlightEngine** dynamically calculates flight statuses based on predefined rules.

#### Flow
1. The system determines the current status of each flight based on predefined rules.
2. The system ensures the status is visible to users.

#### Mermaid Diagram
```mermaid
graph TD
    A[System determines flight status] --> B[System displays status to users]
    B --> C[Status updates dynamically as needed]
```

#### 7. System Health Monitoring
**Flow:**
1. Monitoring system performs health checks on all components.
2. System checks database connectivity, API responsiveness, and SignalR status.
3. Performance metrics are collected and analyzed.
4. Alerts are triggered if thresholds are exceeded.

**Mermaid Diagram:**
```mermaid
graph TD
    A[Health Check Initiated] --> B[Check Database]
    A --> C[Check API Endpoints]
    A --> D[Check SignalR Hub]
    B --> E{Database OK?}
    C --> F{API OK?}
    D --> G{SignalR OK?}
    E -- No --> H[Alert Administrator]
    F -- No --> H
    G -- No --> H
    E -- Yes --> I[System Healthy]
    F -- Yes --> I
    G -- Yes --> I
```

#### 8. Data Backup and Recovery
**Flow:**
1. Automated backup process starts at scheduled time.
2. System creates database backup.
3. Backup is replicated to secondary region.
4. Backup verification is performed.
5. In case of disaster, recovery process is initiated.

**Mermaid Diagram:**
```mermaid
graph TD
    A[Scheduled Backup] --> B[Create Database Backup]
    B --> C[Replicate to Secondary Region]
    C --> D[Verify Backup Integrity]
    D --> E{Backup Valid?}
    E -- Yes --> F[Backup Complete]
    E -- No --> G[Alert & Retry]
    H[Disaster Detected] --> I[Initiate Recovery]
    I --> J[Restore from Backup]
    J --> K[Verify System Health]
```

#### 9. Performance Optimization
**Flow:**
1. User request is received by the system.
2. Cache is checked for existing data.
3. If cache miss, data is retrieved from database.
4. Response is cached for future requests.
5. Performance metrics are logged.

**Mermaid Diagram:**
```mermaid
graph TD
    A[User Request] --> B[Check Cache]
    B --> C{Cache Hit?}
    C -- Yes --> D[Return Cached Data]
    C -- No --> E[Query Database]
    E --> F[Cache Result]
    F --> G[Return Data]
    D --> H[Log Performance Metrics]
    G --> H
```
