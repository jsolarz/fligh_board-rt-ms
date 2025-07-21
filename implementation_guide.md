# Implementation Guide

## Development Contract & Progressive Approach

This guide serves as a development contract (SOW) outlining what needs to be built in iterative, testable phases. Each step is designed to be independently validatable to avoid rework and back-and-forth corrections.

**Core Principles:**
- Build incrementally - each step adds specific functionality
- Test at each milestone before proceeding
- Database migrations separated by feature for easy rollback
- Cloud-agnostic design (Docker deployment option)
- Message queue/bus architecture kept optional for future extension

**Architecture Overview:**
- Backend: .NET Core Web API with Entity Framework
- Frontend: Dual React TypeScript apps (Consumer + Backoffice)
- Database: SQLite for development, production-ready schema
- Real-time: SignalR for live updates
- Authentication: JWT with role-based access control

## Step 1: Project Initialization and Basic Structure

### 1.1 Create Solution Structure
```cmd
mkdir flight-board-system
cd flight-board-system
dotnet new sln -n FlightBoardSystem
```

### 1.2 Create Backend Project
```cmd
cd src
dotnet new webapi -n FlightBoard.Api
cd ..
dotnet sln add src/FlightBoard.Api/FlightBoard.Api.csproj
```

### 1.3 Create Basic Frontend Structure
```cmd
mkdir src/frontend
cd src/frontend
npx create-react-app flight-board-consumer --template typescript
npx create-react-app flight-board-backoffice --template typescript
cd ../..
```

### 1.4 Basic Project Structure Validation
- [ ] Verify solution builds: `dotnet build`
- [ ] Verify frontend projects start: `npm start` in each frontend directory
- [ ] Create initial README.md with setup instructions

## Step 2: Entity Framework Core Setup

### 2.1 Add Entity Framework Core Packages
```cmd
cd src/FlightBoard.Api
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 2.2 Create Database Schema Structure
- [ ] Create `Models/` directory
- [ ] Create base entity model for common properties
- [ ] Create flight entity with proper data annotations

### 2.2.1 Create Base Entity Model
- [ ] Create `Models/BaseEntity.cs`:
  ```csharp
  using System.ComponentModel.DataAnnotations;
  
  public abstract class BaseEntity
  {
      [Key]
      public int Id { get; set; }
      
      [Required]
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
      public DateTime? UpdatedAt { get; set; }
      
      public DateTime? DeletedAt { get; set; } // For soft deletes
      
      public bool IsDeleted { get; set; } = false;
  }
  ```

### 2.2.2 Create Flight Entity with Comprehensive Properties
- [ ] Create `Models/Flight.cs`:
  ```csharp
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  
  public class Flight : BaseEntity
  {
      [Required]
      [StringLength(10, MinimumLength = 3)]
      [Display(Name = "Flight Number")]
      public string FlightNumber { get; set; }
      
      [Required]
      [StringLength(3, MinimumLength = 3)]
      [Display(Name = "Airline Code")]
      public string AirlineCode { get; set; }
      
      [Required]
      [StringLength(100, MinimumLength = 2)]
      public string Destination { get; set; }
      
      [Required]
      [StringLength(100, MinimumLength = 2)]
      public string Origin { get; set; }
      
      [Required]
      [Display(Name = "Scheduled Departure")]
      public DateTime ScheduledDepartureTime { get; set; }
      
      [Display(Name = "Actual Departure")]
      public DateTime? ActualDepartureTime { get; set; }
      
      [Required]
      [StringLength(10, MinimumLength = 1)]
      [RegularExpression(@"^[A-Z][0-9]{1,2}[A-Z]?$", ErrorMessage = "Invalid gate format")]
      public string Gate { get; set; }
      
      [StringLength(10)]
      public string Terminal { get; set; }
      
      [Required]
      [StringLength(20)]
      public string Status { get; set; } // On Time, Delayed, Boarding, Departed, Cancelled
      
      [Range(0, int.MaxValue)]
      public int DelayMinutes { get; set; } = 0;
      
      [StringLength(500)]
      public string Remarks { get; set; }
      
      [StringLength(20)]
      public string AircraftType { get; set; }
      
      // Navigation properties for future relationships
      public int? CreatedByUserId { get; set; }
      public int? LastModifiedByUserId { get; set; }
      
      // Computed properties
      [NotMapped]
      public bool IsDelayed => DelayMinutes > 0;
      
      [NotMapped]
      public DateTime EstimatedDepartureTime => ScheduledDepartureTime.AddMinutes(DelayMinutes);
  }
  ```

### 2.3 Create Enhanced DbContext with Configuration
- [ ] Create `Data/FlightDbContext.cs`:
  ```csharp
  using Microsoft.EntityFrameworkCore;
  using FlightBoard.Api.Models;
  
  public class FlightDbContext : DbContext
  {
      public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options) { }
      
      public DbSet<Flight> Flights { get; set; }
      
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          base.OnModelCreating(modelBuilder);
          
          // Flight entity configuration
          modelBuilder.Entity<Flight>(entity =>
          {
              entity.HasKey(e => e.Id);
              
              // Indexes for performance
              entity.HasIndex(e => e.FlightNumber)
                    .HasDatabaseName("IX_Flight_FlightNumber");
              entity.HasIndex(e => e.ScheduledDepartureTime)
                    .HasDatabaseName("IX_Flight_ScheduledDepartureTime");
              entity.HasIndex(e => e.Destination)
                    .HasDatabaseName("IX_Flight_Destination");
              entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Flight_Status");
              entity.HasIndex(e => e.Gate)
                    .HasDatabaseName("IX_Flight_Gate");
              
              // Composite indexes for common queries
              entity.HasIndex(e => new { e.ScheduledDepartureTime, e.Status })
                    .HasDatabaseName("IX_Flight_DepartureTime_Status");
              entity.HasIndex(e => new { e.Destination, e.ScheduledDepartureTime })
                    .HasDatabaseName("IX_Flight_Destination_DepartureTime");
              
              // Soft delete global filter
              entity.HasQueryFilter(e => !e.IsDeleted);
              
              // Default values
              entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
              entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);
              entity.Property(e => e.DelayMinutes)
                    .HasDefaultValue(0);
          });
      }
      
      public override int SaveChanges()
      {
          UpdateTimestamps();
          return base.SaveChanges();
      }
      
      public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      {
          UpdateTimestamps();
          return await base.SaveChangesAsync(cancellationToken);
      }
      
      private void UpdateTimestamps()
      {
          var entries = ChangeTracker.Entries()
              .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));
          
          foreach (var entry in entries)
          {
              var entity = (BaseEntity)entry.Entity;
              
              if (entry.State == EntityState.Added)
              {
                  entity.CreatedAt = DateTime.UtcNow;
              }
              else if (entry.State == EntityState.Modified)
              {
                  entity.UpdatedAt = DateTime.UtcNow;
              }
          }
      }
  }
  ```

### 2.4 Configure Database Connection
- [ ] Add connection string to `appsettings.json`:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=flightboard.db"
    }
  }
  ```
- [ ] Register DbContext in `Program.cs`:
  ```csharp
  builder.Services.AddDbContext<FlightDbContext>(options =>
      options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
  ```

### 2.5 Create Initial Migration with Comprehensive Schema
```cmd
dotnet ef migrations add InitialFlightSchema -o Data/Migrations
dotnet ef database update
```

### 2.5.1 Migration Content Verification
The migration should include:
- [ ] `Flights` table with all columns from Flight entity
- [ ] Primary key constraint on `Id`
- [ ] All indexes defined in DbContext
- [ ] Default value constraints
- [ ] Check constraints for data validation
- [ ] Proper column types and lengths

### 2.5.2 Add Database Seeding
- [ ] Create `Data/DatabaseSeeder.cs`:
  ```csharp
  using FlightBoard.Api.Models;
  using Microsoft.EntityFrameworkCore;
  
  public static class DatabaseSeeder
  {
      public static async Task SeedAsync(FlightDbContext context)
      {
          if (await context.Flights.AnyAsync())
              return; // Database already seeded
          
          var sampleFlights = new List<Flight>
          {
              new Flight
              {
                  FlightNumber = "AA123",
                  AirlineCode = "AA",
                  Origin = "New York",
                  Destination = "Los Angeles",
                  ScheduledDepartureTime = DateTime.UtcNow.AddHours(2),
                  Gate = "A12",
                  Terminal = "1",
                  Status = "On Time",
                  AircraftType = "Boeing 737"
              },
              new Flight
              {
                  FlightNumber = "DL456",
                  AirlineCode = "DL",
                  Origin = "Atlanta",
                  Destination = "Miami",
                  ScheduledDepartureTime = DateTime.UtcNow.AddHours(3),
                  Gate = "B5",
                  Terminal = "2",
                  Status = "Delayed",
                  DelayMinutes = 15,
                  AircraftType = "Airbus A320"
              },
              new Flight
              {
                  FlightNumber = "UA789",
                  AirlineCode = "UA",
                  Origin = "Chicago",
                  Destination = "Denver",
                  ScheduledDepartureTime = DateTime.UtcNow.AddHours(1),
                  Gate = "C8",
                  Terminal = "3",
                  Status = "Boarding",
                  AircraftType = "Boeing 757"
              }
          };
          
          await context.Flights.AddRangeAsync(sampleFlights);
          await context.SaveChangesAsync();
      }
  }
  ```

### 2.5.3 Register Seeding in Program.cs
- [ ] Add seeding call in `Program.cs`:
  ```csharp
  // After app configuration, before app.Run()
  using (var scope = app.Services.CreateScope())
  {
      var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
      await DatabaseSeeder.SeedAsync(context);
  }
  ```

### 2.6 Comprehensive Database Verification
- [ ] Check that `flightboard.db` file is created in project root
- [ ] Verify database schema with SQLite browser or command line:
  ```cmd
  sqlite3 flightboard.db ".schema Flights"
  ```
- [ ] Validate all indexes are created:
  ```cmd
  sqlite3 flightboard.db ".indices Flights"
  ```
- [ ] Test database connection and seeded data:
  ```cmd
  sqlite3 flightboard.db "SELECT COUNT(*) FROM Flights;"
  ```
- [ ] Verify audit fields are working (CreatedAt, UpdatedAt)
- [ ] Test soft delete functionality
- [ ] Validate foreign key constraints (if any)

### 2.7 Database Performance Baseline
- [ ] Create `Data/DatabaseDiagnostics.cs` for performance monitoring:
  ```csharp
  public static class DatabaseDiagnostics
  {
      public static async Task<DatabaseStats> GetStatsAsync(FlightDbContext context)
      {
          return new DatabaseStats
          {
              FlightCount = await context.Flights.CountAsync(),
              ActiveFlightCount = await context.Flights.Where(f => !f.IsDeleted).CountAsync(),
              DelayedFlightCount = await context.Flights.Where(f => f.DelayMinutes > 0).CountAsync(),
              DatabaseSizeKB = GetDatabaseSize()
          };
      }
      
      private static long GetDatabaseSize()
      {
          var dbPath = "flightboard.db";
          return File.Exists(dbPath) ? new FileInfo(dbPath).Length / 1024 : 0;
      }
  }
  
  public class DatabaseStats
  {
      public int FlightCount { get; set; }
      public int ActiveFlightCount { get; set; }
      public int DelayedFlightCount { get; set; }
      public long DatabaseSizeKB { get; set; }
  }
  ```

## Step 3: Basic API Endpoints

### 3.1 Create Flight Controller
- [ ] Create `Controllers/FlightsController.cs`
- [ ] Implement basic GET endpoint to retrieve all flights
- [ ] Add simple POST endpoint to create a flight
- [ ] Test endpoints with Swagger UI

### 3.2 Add Basic Validation
- [ ] Add data annotations to Flight model
- [ ] Implement basic model validation in controller
- [ ] Return appropriate HTTP status codes

### 3.3 Test and Validate
- [ ] Use Swagger/Postman to test all endpoints
- [ ] Verify data is persisted correctly
- [ ] Test validation rules

### 3.3 Add Data Annotations and Validation
- [ ] Update `Models/Flight.cs` with validation attributes:
  ```csharp
  [Required]
  [StringLength(10)]
  public string FlightNumber { get; set; }
  
  [Required]
  [StringLength(100)]
  public string Destination { get; set; }
  
  [Required]
  public DateTime DepartureTime { get; set; }
  
  [Required]
  [StringLength(10)]
  public string Gate { get; set; }
  
  [Required]
  [StringLength(20)]
  public string Status { get; set; }
  ```

## Step 4: Basic Frontend - Consumer App

### 4.1 Setup Dependencies
```cmd
cd src/frontend/flight-board-consumer
npm install axios @tanstack/react-query
```

### 4.2 Create Basic Flight Display
- [ ] Create `types/Flight.ts` interface
- [ ] Create `services/flightApi.ts` for API calls
- [ ] Create `components/FlightBoard.tsx` to display flights in a table
- [ ] Setup React Query provider in `App.tsx`

### 4.2 Create Detailed Flight Display Components
- [ ] Create `types/Flight.ts` interface:
  ```typescript
  export interface Flight {
    flightId: number;
    flightNumber: string;
    destination: string;
    departureTime: string;
    gate: string;
    status: string;
  }
  ```

- [ ] Create `services/flightApi.ts` for API calls:
  ```typescript
  import axios from 'axios';
  import { Flight } from '../types/Flight';

  const API_BASE = 'https://localhost:7000/api';

  export const flightApi = {
    getAll: () => axios.get<Flight[]>(`${API_BASE}/flights`),
    create: (flight: Omit<Flight, 'flightId'>) => axios.post<Flight>(`${API_BASE}/flights`, flight),
    delete: (id: number) => axios.delete(`${API_BASE}/flights/${id}`),
    search: (filters: SearchFilters) => {
      const params = new URLSearchParams();
      Object.entries(filters).forEach(([key, value]) => {
        if (value) params.append(key, value);
      });
      return axios.get<Flight[]>(`${API_BASE}/flights/search?${params.toString()}`);
    }
  };
  ```

- [ ] Create `components/FlightBoard.tsx` with detailed table:
  ```typescript
  import React from 'react';
  import { useQuery } from '@tanstack/react-query';
  import { flightApi } from '../services/flightApi';

  export const FlightBoard: React.FC = () => {
    const { data: flights, isLoading, error } = useQuery({
      queryKey: ['flights'],
      queryFn: () => flightApi.getAll().then(res => res.data)
    });

    if (isLoading) return <div>Loading flights...</div>;
    if (error) return <div>Error loading flights</div>;

    return (
      <table className="flight-board-table">
        <thead>
          <tr>
            <th>Flight Number</th>
            <th>Destination</th>
            <th>Departure Time</th>
            <th>Gate</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {flights?.map(flight => (
            <tr key={flight.flightId}>
              <td>{flight.flightNumber}</td>
              <td>{flight.destination}</td>
              <td>{new Date(flight.departureTime).toLocaleString()}</td>
              <td>{flight.gate}</td>
              <td>{flight.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };
  ```

### 4.3 Basic Functionality
- [ ] Fetch and display flights from API
- [ ] Add basic error handling
- [ ] Add loading states

### 4.4 Test Integration
- [ ] Verify frontend can fetch data from backend
- [ ] Test with sample flight data

## Step 5: CORS and API Configuration

### 5.1 Configure CORS
- [ ] Add CORS configuration in backend `Program.cs`
- [ ] Allow frontend origins (localhost:3000, localhost:3001)
- [ ] Test cross-origin requests

### 5.2 Environment Configuration
- [ ] Create `appsettings.Development.json`
- [ ] Add environment-specific settings
- [ ] Configure API base URL in frontend

## Step 6: Real-time Updates with SignalR

### 6.1 Add SignalR to Backend
```cmd
cd src/FlightBoard.Api
dotnet add package Microsoft.AspNetCore.SignalR
```

### 6.2 Create SignalR Hub
- [ ] Create `Hubs/FlightHub.cs`
- [ ] Configure SignalR in `Program.cs`
- [ ] Add SignalR endpoints to controller to broadcast updates

### 6.3 Add SignalR to Frontend
```cmd
cd src/frontend/flight-board-consumer
npm install @microsoft/signalr
```

### 6.4 Implement Detailed Real-time Updates
- [ ] Create `services/signalRConnection.ts`:
  ```typescript
  import * as signalR from '@microsoft/signalr';

  class SignalRService {
    private connection: signalR.HubConnection;

    constructor() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7000/flightHub')
        .withAutomaticReconnect()
        .build();
    }

    public async start(): Promise<void> {
      try {
        await this.connection.start();
        console.log('SignalR Connected');
      } catch (err) {
        console.error('SignalR Connection Error: ', err);
      }
    }

    public onFlightUpdate(callback: (flight: Flight) => void): void {
      this.connection.on('FlightUpdated', callback);
    }

    public onFlightAdded(callback: (flight: Flight) => void): void {
      this.connection.on('FlightAdded', callback);
    }

    public onFlightDeleted(callback: (flightId: number) => void): void {
      this.connection.on('FlightDeleted', callback);
    }

    public async stop(): Promise<void> {
      await this.connection.stop();
    }
  }

  export const signalRService = new SignalRService();
  ```

- [ ] Update FlightBoard component to use SignalR:
  ```typescript
  useEffect(() => {
    signalRService.start();
    
    signalRService.onFlightAdded((flight) => {
      queryClient.setQueryData(['flights'], (old: Flight[]) => [...old, flight]);
    });

    signalRService.onFlightUpdated((flight) => {
      queryClient.setQueryData(['flights'], (old: Flight[]) => 
        old.map(f => f.flightId === flight.flightId ? flight : f)
      );
    });

    signalRService.onFlightDeleted((flightId) => {
      queryClient.setQueryData(['flights'], (old: Flight[]) => 
        old.filter(f => f.flightId !== flightId)
      );
    });

    return () => {
      signalRService.stop();
    };
  }, [queryClient]);
  ```

## Step 7: Flight Management (Add/Delete)

### 7.1 Extend Backend API
- [ ] Add DELETE endpoint for flights
- [ ] Add PUT endpoint for flight updates
- [ ] Enhance validation and error handling
- [ ] Add SignalR notifications for all operations

### 7.2 Create Admin Interface (Backoffice Frontend)
```cmd
cd src/frontend/flight-board-backoffice
npm install axios @tanstack/react-query @microsoft/signalr
```

### 7.3 Build Management UI
- [ ] Create flight creation form
- [ ] Add flight list with edit/delete actions
- [ ] Implement form validation
- [ ] Add confirmation dialogs for delete operations

### 7.3 Build Detailed Management UI
- [ ] Create `components/FlightForm.tsx`:
  ```typescript
  import React, { useState } from 'react';
  import { useMutation, useQueryClient } from '@tanstack/react-query';
  import { flightApi } from '../services/flightApi';

  export const FlightForm: React.FC = () => {
    const [formData, setFormData] = useState({
      flightNumber: '',
      destination: '',
      departureTime: '',
      gate: '',
      status: 'Scheduled'
    });

    const queryClient = useQueryClient();
    const createMutation = useMutation({
      mutationFn: flightApi.create,
      onSuccess: () => {
        queryClient.invalidateQueries(['flights']);
        setFormData({
          flightNumber: '',
          destination: '',
          departureTime: '',
          gate: '',
          status: 'Scheduled'
        });
      }
    });

    const handleSubmit = (e: React.FormEvent) => {
      e.preventDefault();
      createMutation.mutate(formData);
    };

    return (
      <form onSubmit={handleSubmit} className="flight-form">
        <div>
          <label>Flight Number:</label>
          <input
            type="text"
            value={formData.flightNumber}
            onChange={(e) => setFormData({...formData, flightNumber: e.target.value})}
            required
          />
        </div>
        <div>
          <label>Destination:</label>
          <input
            type="text"
            value={formData.destination}
            onChange={(e) => setFormData({...formData, destination: e.target.value})}
            required
          />
        </div>
        <div>
          <label>Departure Time:</label>
          <input
            type="datetime-local"
            value={formData.departureTime}
            onChange={(e) => setFormData({...formData, departureTime: e.target.value})}
            required
          />
        </div>
        <div>
          <label>Gate:</label>
          <input
            type="text"
            value={formData.gate}
            onChange={(e) => setFormData({...formData, gate: e.target.value})}
            required
          />
        </div>
        <div>
          <label>Status:</label>
          <select
            value={formData.status}
            onChange={(e) => setFormData({...formData, status: e.target.value})}
          >
            <option value="Scheduled">Scheduled</option>
            <option value="Boarding">Boarding</option>
            <option value="Departed">Departed</option>
            <option value="Landed">Landed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <button type="submit" disabled={createMutation.isLoading}>
          {createMutation.isLoading ? 'Adding...' : 'Add Flight'}
        </button>
      </form>
    );
  };
  ```

- [ ] Create `components/FlightList.tsx` with edit/delete actions:
  ```typescript
  import React from 'react';
  import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
  import { flightApi } from '../services/flightApi';

  export const FlightList: React.FC = () => {
    const queryClient = useQueryClient();
    const { data: flights } = useQuery({
      queryKey: ['flights'],
      queryFn: () => flightApi.getAll().then(res => res.data)
    });

    const deleteMutation = useMutation({
      mutationFn: flightApi.delete,
      onSuccess: () => {
        queryClient.invalidateQueries(['flights']);
      }
    });

    const handleDelete = (id: number) => {
      if (window.confirm('Are you sure you want to delete this flight?')) {
        deleteMutation.mutate(id);
      }
    };

    return (
      <div className="flight-list">
        <h3>Manage Flights</h3>
        <table>
          <thead>
            <tr>
              <th>Flight Number</th>
              <th>Destination</th>
              <th>Departure</th>
              <th>Gate</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {flights?.map(flight => (
              <tr key={flight.flightId}>
                <td>{flight.flightNumber}</td>
                <td>{flight.destination}</td>
                <td>{new Date(flight.departureTime).toLocaleString()}</td>
                <td>{flight.gate}</td>
                <td>{flight.status}</td>
                <td>
                  <button onClick={() => handleDelete(flight.flightId)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  };
  ```

### 7.4 Test Complete CRUD Operations
- [ ] Test adding flights from backoffice
- [ ] Verify real-time updates appear in consumer app
- [ ] Test editing and deleting flights

## Step 8: Search and Filtering

### 8.1 Enhanced Backend Search
- [ ] Add search endpoint with query parameters
- [ ] Implement filtering by destination, status, date range
- [ ] Add database indexing for performance

### 8.2 Frontend Search UI
- [ ] Add search form to consumer app
- [ ] Implement filter dropdowns
- [ ] Add clear filters functionality
- [ ] Debounce search input for performance

### 8.2 Detailed Frontend Search Implementation
- [ ] Create `components/SearchFilters.tsx`:
  ```typescript
  import React, { useState, useCallback } from 'react';
  import { debounce } from 'lodash';

  interface SearchFiltersProps {
    onSearchChange: (filters: SearchFilters) => void;
  }

  interface SearchFilters {
    destination?: string;
    status?: string;
    dateFrom?: string;
    dateTo?: string;
  }

  export const SearchFilters: React.FC<SearchFiltersProps> = ({ onSearchChange }) => {
    const [filters, setFilters] = useState<SearchFilters>({});

    const debouncedSearch = useCallback(
      debounce((newFilters: SearchFilters) => {
        onSearchChange(newFilters);
      }, 300),
      [onSearchChange]
    );

    const updateFilters = (key: keyof SearchFilters, value: string) => {
      const newFilters = { ...filters, [key]: value || undefined };
      setFilters(newFilters);
      debouncedSearch(newFilters);
    };

    const clearFilters = () => {
      setFilters({});
      onSearchChange({});
    };

    return (
      <div className="search-filters">
        <div className="filter-group">
          <label>Destination:</label>
          <input
            type="text"
            placeholder="Search destination..."
            value={filters.destination || ''}
            onChange={(e) => updateFilters('destination', e.target.value)}
          />
        </div>
        
        <div className="filter-group">
          <label>Status:</label>
          <select
            value={filters.status || ''}
            onChange={(e) => updateFilters('status', e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="Scheduled">Scheduled</option>
            <option value="Boarding">Boarding</option>
            <option value="Departed">Departed</option>
            <option value="Landed">Landed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>

        <div className="filter-group">
          <label>Date From:</label>
          <input
            type="date"
            value={filters.dateFrom || ''}
            onChange={(e) => updateFilters('dateFrom', e.target.value)}
          />
        </div>

        <div className="filter-group">
          <label>Date To:</label>
          <input
            type="date"
            value={filters.dateTo || ''}
            onChange={(e) => updateFilters('dateTo', e.target.value)}
          />
        </div>

        <button onClick={clearFilters} className="clear-filters">
          Clear Filters
        </button>
      </div>
    );
  };
  ```

### 8.3 Advanced Search Features
- [ ] Add pagination for large result sets
- [ ] Implement sorting by different columns
- [ ] Add search result highlighting

## Step 8.4: Enhanced Database Optimization for Search
```cmd
cd src/FlightBoard.Api
dotnet ef migrations add OptimizeSearchIndexes -o Data/Migrations
dotnet ef database update
```

### 8.4.1 Advanced Search Index Migration
The migration should include:
- [ ] Full-text search indexes for string columns
- [ ] Composite indexes for complex filter combinations
- [ ] Covering indexes to avoid key lookups
- [ ] Partial indexes for active flights only

### 8.4.2 Create Advanced Search Indexes
- [ ] Update `FlightDbContext.cs` OnModelCreating method:
  ```csharp
  // Add to existing OnModelCreating method
  
  // Advanced search indexes
  entity.HasIndex(e => new { e.AirlineCode, e.ScheduledDepartureTime })
        .HasDatabaseName("IX_Flight_Airline_DepartureTime");
  
  entity.HasIndex(e => new { e.Origin, e.Destination, e.ScheduledDepartureTime })
        .HasDatabaseName("IX_Flight_Route_DepartureTime");
  
  entity.HasIndex(e => new { e.Status, e.Gate, e.Terminal })
        .HasDatabaseName("IX_Flight_Status_Gate_Terminal");
  
  // Covering index for flight board display
  entity.HasIndex(e => new { e.ScheduledDepartureTime, e.FlightNumber, e.Destination, e.Gate, e.Status })
        .HasDatabaseName("IX_Flight_BoardDisplay_Covering");
  
  // Partial index for active flights (SQLite doesn't support filtered indexes, but we can document intent)
  entity.HasIndex(e => new { e.IsDeleted, e.ScheduledDepartureTime })
        .HasDatabaseName("IX_Flight_Active_DepartureTime");
  ```

### 8.4.3 Database Query Performance Testing
- [ ] Create `Data/QueryPerformanceTests.cs`:
  ```csharp
  public class QueryPerformanceTests
  {
      public static async Task<PerformanceReport> RunTestsAsync(FlightDbContext context)
      {
          var stopwatch = Stopwatch.StartNew();
          var report = new PerformanceReport();
          
          // Test 1: Simple flight lookup
          stopwatch.Restart();
          var flight = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "AA123");
          report.FlightLookupMs = stopwatch.ElapsedMilliseconds;
          
          // Test 2: Date range query
          stopwatch.Restart();
          var todayFlights = await context.Flights
              .Where(f => f.ScheduledDepartureTime.Date == DateTime.Today)
              .ToListAsync();
          report.DateRangeQueryMs = stopwatch.ElapsedMilliseconds;
          
          // Test 3: Complex filter query
          stopwatch.Restart();
          var filteredFlights = await context.Flights
              .Where(f => f.Destination.Contains("Los") && f.Status == "On Time")
              .OrderBy(f => f.ScheduledDepartureTime)
              .ToListAsync();
          report.ComplexFilterMs = stopwatch.ElapsedMilliseconds;
          
          // Test 4: Aggregation query
          stopwatch.Restart();
          var stats = await context.Flights
              .GroupBy(f => f.Status)
              .Select(g => new { Status = g.Key, Count = g.Count() })
              .ToListAsync();
          report.AggregationQueryMs = stopwatch.ElapsedMilliseconds;
          
          return report;
      }
  }
  
  public class PerformanceReport
  {
      public long FlightLookupMs { get; set; }
      public long DateRangeQueryMs { get; set; }
      public long ComplexFilterMs { get; set; }
      public long AggregationQueryMs { get; set; }
  }
  ```

### 8.4.4 Verify Index Effectiveness
- [ ] Test query performance before and after index creation
- [ ] Use SQLite EXPLAIN QUERY PLAN to verify index usage:
  ```sql
  EXPLAIN QUERY PLAN SELECT * FROM Flights WHERE FlightNumber = 'AA123';
  EXPLAIN QUERY PLAN SELECT * FROM Flights WHERE ScheduledDepartureTime BETWEEN datetime('now') AND datetime('now', '+1 day');
  ```
- [ ] Document performance improvements in comments

## Step 9: Authentication Foundation

### 9.1 User Management Database
- [ ] Create User, Role, and UserRole entities
- [ ] Add authentication-related migrations
- [ ] Update DbContext with new entities

### 9.2 JWT Authentication Setup
```cmd
cd src/FlightBoard.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 9.3 Basic Authentication
- [ ] Create authentication controller
- [ ] Implement login endpoint
- [ ] Add JWT token generation
- [ ] Create user registration (for testing)

### 9.4 Frontend Authentication
- [ ] Create login components for both apps
- [ ] Implement token storage and management
- [ ] Add authentication context/state management

## Step 9.4: Comprehensive Authentication Database Schema
```cmd
cd src/FlightBoard.Api
dotnet ef migrations add AddCompleteUserAuthenticationSystem -o Data/Migrations
dotnet ef database update
```

### 9.4.1 Create Complete Authentication Models
- [ ] Create `Models/User.cs`:
  ```csharp
  public class User : BaseEntity
  {
      [Required]
      [StringLength(50, MinimumLength = 3)]
      public string Username { get; set; }
      
      [Required]
      [EmailAddress]
      [StringLength(255)]
      public string Email { get; set; }
      
      [Required]
      [StringLength(255)]
      public string PasswordHash { get; set; }
      
      [StringLength(100)]
      public string FirstName { get; set; }
      
      [StringLength(100)]
      public string LastName { get; set; }
      
      public bool IsActive { get; set; } = true;
      public bool EmailConfirmed { get; set; } = false;
      public DateTime? LastLoginAt { get; set; }
      public int FailedLoginAttempts { get; set; } = 0;
      public DateTime? LockedOutUntil { get; set; }
      
      [StringLength(255)]
      public string RefreshToken { get; set; }
      public DateTime? RefreshTokenExpiryTime { get; set; }
      
      // Navigation properties
      public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
      public virtual ICollection<Flight> CreatedFlights { get; set; } = new List<Flight>();
      public virtual ICollection<Flight> ModifiedFlights { get; set; } = new List<Flight>();
  }
  ```

- [ ] Create `Models/Role.cs`:
  ```csharp
  public class Role : BaseEntity
  {
      [Required]
      [StringLength(50)]
      public string Name { get; set; }
      
      [StringLength(255)]
      public string Description { get; set; }
      
      public bool IsSystemRole { get; set; } = false;
      
      // Navigation properties
      public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
      public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
  }
  ```

- [ ] Create `Models/UserRole.cs`:
  ```csharp
  public class UserRole : BaseEntity
  {
      [Required]
      public int UserId { get; set; }
      
      [Required]
      public int RoleId { get; set; }
      
      public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
      public int? AssignedByUserId { get; set; }
      public DateTime? ExpiresAt { get; set; }
      
      // Navigation properties
      public virtual User User { get; set; }
      public virtual Role Role { get; set; }
      public virtual User AssignedBy { get; set; }
  }
  ```

- [ ] Create `Models/Permission.cs`:
  ```csharp
  public class Permission : BaseEntity
  {
      [Required]
      [StringLength(100)]
      public string Name { get; set; }
      
      [Required]
      [StringLength(50)]
      public string Category { get; set; } // Flight, User, System, Report
      
      [StringLength(255)]
      public string Description { get; set; }
      
      // Navigation properties
      public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
  }
  ```

- [ ] Create `Models/RolePermission.cs`:
  ```csharp
  public class RolePermission : BaseEntity
  {
      [Required]
      public int RoleId { get; set; }
      
      [Required]
      public int PermissionId { get; set; }
      
      // Navigation properties
      public virtual Role Role { get; set; }
      public virtual Permission Permission { get; set; }
  }
  ```

### 9.4.2 Update DbContext with Authentication Entities
- [ ] Add to `FlightDbContext.cs`:
  ```csharp
  // Add these DbSets
  public DbSet<User> Users { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }
  public DbSet<Permission> Permissions { get; set; }
  public DbSet<RolePermission> RolePermissions { get; set; }
  
  // Add to OnModelCreating method
  ConfigureAuthenticationEntities(modelBuilder);
  
  private void ConfigureAuthenticationEntities(ModelBuilder modelBuilder)
  {
      // User configuration
      modelBuilder.Entity<User>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("IX_User_Username");
          entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_User_Email");
          entity.HasIndex(e => e.RefreshToken).HasDatabaseName("IX_User_RefreshToken");
          entity.HasQueryFilter(e => !e.IsDeleted);
      });
      
      // Role configuration
      modelBuilder.Entity<Role>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_Role_Name");
          entity.HasQueryFilter(e => !e.IsDeleted);
      });
      
      // UserRole configuration
      modelBuilder.Entity<UserRole>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique().HasDatabaseName("IX_UserRole_User_Role");
          
          entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
          entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
          entity.HasOne(ur => ur.AssignedBy)
                .WithMany()
                .HasForeignKey(ur => ur.AssignedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
                
          entity.HasQueryFilter(e => !e.IsDeleted);
      });
      
      // Permission configuration
      modelBuilder.Entity<Permission>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_Permission_Name");
          entity.HasIndex(e => e.Category).HasDatabaseName("IX_Permission_Category");
          entity.HasQueryFilter(e => !e.IsDeleted);
      });
      
      // RolePermission configuration
      modelBuilder.Entity<RolePermission>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique().HasDatabaseName("IX_RolePermission_Role_Permission");
          
          entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
          entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
          entity.HasQueryFilter(e => !e.IsDeleted);
      });
      
      // Update Flight entity for user relationships
      modelBuilder.Entity<Flight>(entity =>
      {
          entity.HasOne<User>()
                .WithMany(u => u.CreatedFlights)
                .HasForeignKey(f => f.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
                
          entity.HasOne<User>()
                .WithMany(u => u.ModifiedFlights)
                .HasForeignKey(f => f.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
      });
  }
  ```

### 9.4.3 Create Authentication Data Seeder
- [ ] Create `Data/AuthenticationSeeder.cs`:
  ```csharp
  public static class AuthenticationSeeder
  {
      public static async Task SeedAsync(FlightDbContext context)
      {
          if (await context.Users.AnyAsync())
              return; // Already seeded
          
          // Seed permissions
          var permissions = new List<Permission>
          {
              new Permission { Name = "Flight.Create", Category = "Flight", Description = "Create new flights" },
              new Permission { Name = "Flight.Read", Category = "Flight", Description = "View flights" },
              new Permission { Name = "Flight.Update", Category = "Flight", Description = "Update flight information" },
              new Permission { Name = "Flight.Delete", Category = "Flight", Description = "Delete flights" },
              new Permission { Name = "User.Create", Category = "User", Description = "Create new users" },
              new Permission { Name = "User.Read", Category = "User", Description = "View users" },
              new Permission { Name = "User.Update", Category = "User", Description = "Update user information" },
              new Permission { Name = "User.Delete", Category = "User", Description = "Delete users" },
              new Permission { Name = "System.Admin", Category = "System", Description = "Full system administration" },
              new Permission { Name = "Report.Generate", Category = "Report", Description = "Generate reports" }
          };
          
          await context.Permissions.AddRangeAsync(permissions);
          await context.SaveChangesAsync();
          
          // Seed roles
          var adminRole = new Role { Name = "Administrator", Description = "Full system access", IsSystemRole = true };
          var operatorRole = new Role { Name = "Flight Operator", Description = "Flight management access", IsSystemRole = true };
          var viewerRole = new Role { Name = "Viewer", Description = "Read-only access", IsSystemRole = true };
          
          await context.Roles.AddRangeAsync(new[] { adminRole, operatorRole, viewerRole });
          await context.SaveChangesAsync();
          
          // Assign permissions to roles
          var rolePermissions = new List<RolePermission>();
          
          // Admin gets all permissions
          foreach (var permission in permissions)
          {
              rolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = permission.Id });
          }
          
          // Operator gets flight and report permissions
          var operatorPermissions = permissions.Where(p => p.Category == "Flight" || p.Name == "Report.Generate");
          foreach (var permission in operatorPermissions)
          {
              rolePermissions.Add(new RolePermission { RoleId = operatorRole.Id, PermissionId = permission.Id });
          }
          
          // Viewer gets read permissions only
          var viewerPermissions = permissions.Where(p => p.Name.EndsWith(".Read"));
          foreach (var permission in viewerPermissions)
          {
              rolePermissions.Add(new RolePermission { RoleId = viewerRole.Id, PermissionId = permission.Id });
          }
          
          await context.RolePermissions.AddRangeAsync(rolePermissions);
          await context.SaveChangesAsync();
          
          // Create default admin user
          var adminUser = new User
          {
              Username = "admin",
              Email = "admin@flightboard.com",
              PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
              FirstName = "System",
              LastName = "Administrator",
              IsActive = true,
              EmailConfirmed = true
          };
          
          await context.Users.AddAsync(adminUser);
          await context.SaveChangesAsync();
          
          // Assign admin role to admin user
          var adminUserRole = new UserRole
          {
              UserId = adminUser.Id,
              RoleId = adminRole.Id,
              AssignedAt = DateTime.UtcNow
          };
          
          await context.UserRoles.AddAsync(adminUserRole);
          await context.SaveChangesAsync();
      }
  }
  ```

### 9.4.4 Migration Verification and Testing
- [ ] Verify all authentication tables are created with proper relationships
- [ ] Test cascade delete behavior
- [ ] Validate unique constraints on usernames and emails
- [ ] Test soft delete functionality across all entities
- [ ] Verify seeded data is properly inserted
- [ ] Test foreign key constraints

## Step 10: Role-Based Access Control

### 10.1 Authorization Implementation
- [ ] Add authorization policies to backend
- [ ] Implement role-based endpoint protection
- [ ] Create middleware for role checking

### 10.2 Frontend Route Protection
- [ ] Implement route guards
- [ ] Create role-based component rendering
- [ ] Add unauthorized access handling

### 10.3 Application Separation
- [ ] Restrict backoffice to admin users only
- [ ] Allow admins to access consumer app
- [ ] Test access control scenarios

## Step 11: Flight Status Calculation

### 11.1 Business Logic Implementation
- [ ] Create flight status calculation service
- [ ] Implement status rules (Scheduled, Boarding, Departed, Landed)
- [ ] Add background service for status updates

### 11.2 Real-time Status Updates
- [ ] Schedule automatic status calculations
- [ ] Broadcast status changes via SignalR
- [ ] Update UI to reflect status changes with visual indicators

## Step 12: Error Handling and Logging

### 12.1 Structured Logging
```cmd
cd src/FlightBoard.Api
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

### 12.2 Error Handling Middleware
- [ ] Create global exception handling middleware
- [ ] Implement standardized error responses
- [ ] Add request/response logging

### 12.3 Frontend Error Handling
- [ ] Add React error boundaries
- [ ] Implement global error handling for API calls
- [ ] Add user-friendly error messages

## Step 13: Basic Testing

### 13.1 Backend Unit Tests
```cmd
mkdir tests
cd tests
dotnet new xunit -n FlightBoard.Api.Tests
cd ..
dotnet sln add tests/FlightBoard.Api.Tests/FlightBoard.Api.Tests.csproj
```

### 13.2 Test Implementation
- [ ] Add tests for flight controller
- [ ] Test authentication and authorization
- [ ] Add integration tests for database operations

### 13.3 Frontend Tests
```cmd
cd src/frontend/flight-board-consumer
npm install --save-dev @testing-library/jest-dom @testing-library/user-event
```

### 13.4 Component Testing
- [ ] Test flight board component
- [ ] Test authentication flows
- [ ] Add integration tests for API communication

## Step 14: Performance Optimization

### 14.1 Caching Implementation
```cmd
cd src/FlightBoard.Api
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

### 14.2 Backend Caching
- [ ] Add Redis configuration
- [ ] Implement caching for flight data
- [ ] Add cache invalidation on data updates
- [ ] Cache search results

### 14.3 Frontend Optimization
- [ ] Implement React.memo for components
- [ ] Add lazy loading for routes
- [ ] Optimize re-renders with useMemo and useCallback

## Step 15: Database Optimization

### 15.1 Indexing and Performance
- [ ] Add database indexes for frequently queried columns
- [ ] Optimize flight search queries
- [ ] Add database performance monitoring

### 15.2 Migration to Production Database
- [ ] Configure SQL Server connection
- [ ] Update connection strings for different environments
- [ ] Test migration scripts

## Step 15.3: Production Database Schema Optimization
```cmd
cd src/FlightBoard.Api
dotnet ef migrations add ProductionOptimizations -o Data/Migrations
dotnet ef database update
```

### 15.3.1 Advanced Performance Optimizations
- [ ] Create additional composite indexes for complex queries:
  ```csharp
  // Add to FlightDbContext OnModelCreating
  
  // Operational dashboard queries
  entity.HasIndex(e => new { e.ScheduledDepartureTime, e.Status, e.Gate })
        .HasDatabaseName("IX_Flight_Dashboard_Composite");
  
  // Airline-specific queries
  entity.HasIndex(e => new { e.AirlineCode, e.Status, e.ScheduledDepartureTime })
        .HasDatabaseName("IX_Flight_Airline_Status_Time");
  
  // Historical reporting queries
  entity.HasIndex(e => new { e.CreatedAt, e.Status })
        .HasDatabaseName("IX_Flight_Historical_Status");
  
  // User activity tracking
  entity.HasIndex(e => new { e.CreatedByUserId, e.CreatedAt })
        .HasDatabaseName("IX_Flight_Creator_Time");
  entity.HasIndex(e => new { e.LastModifiedByUserId, e.UpdatedAt })
        .HasDatabaseName("IX_Flight_Modifier_Time");
  ```

### 15.3.2 Database Views for Complex Queries
- [ ] Create `Data/DatabaseViews.cs`:
  ```csharp
  public static class DatabaseViews
  {
      public static async Task CreateViewsAsync(FlightDbContext context)
      {
          // Flight dashboard view
          await context.Database.ExecuteSqlRawAsync(@"
              CREATE VIEW IF NOT EXISTS FlightDashboardView AS
              SELECT 
                  f.Id,
                  f.FlightNumber,
                  f.AirlineCode,
                  f.Origin,
                  f.Destination,
                  f.ScheduledDepartureTime,
                  f.ActualDepartureTime,
                  f.Gate,
                  f.Terminal,
                  f.Status,
                  f.DelayMinutes,
                  f.AircraftType,
                  CASE 
                      WHEN f.DelayMinutes > 0 THEN datetime(f.ScheduledDepartureTime, '+' || f.DelayMinutes || ' minutes')
                      ELSE f.ScheduledDepartureTime
                  END as EstimatedDepartureTime,
                  cu.Username as CreatedBy,
                  mu.Username as LastModifiedBy
              FROM Flights f
              LEFT JOIN Users cu ON f.CreatedByUserId = cu.Id
              LEFT JOIN Users mu ON f.LastModifiedByUserId = mu.Id
              WHERE f.IsDeleted = 0
          ");
          
          // User activity summary view
          await context.Database.ExecuteSqlRawAsync(@"
              CREATE VIEW IF NOT EXISTS UserActivityView AS
              SELECT 
                  u.Id,
                  u.Username,
                  u.Email,
                  u.FirstName,
                  u.LastName,
                  u.LastLoginAt,
                  COUNT(DISTINCT f1.Id) as FlightsCreated,
                  COUNT(DISTINCT f2.Id) as FlightsModified,
                  GROUP_CONCAT(DISTINCT r.Name) as Roles
              FROM Users u
              LEFT JOIN Flights f1 ON u.Id = f1.CreatedByUserId AND f1.IsDeleted = 0
              LEFT JOIN Flights f2 ON u.Id = f2.LastModifiedByUserId AND f2.IsDeleted = 0
              LEFT JOIN UserRoles ur ON u.Id = ur.UserId AND ur.IsDeleted = 0
              LEFT JOIN Roles r ON ur.RoleId = r.Id AND r.IsDeleted = 0
              WHERE u.IsDeleted = 0
              GROUP BY u.Id, u.Username, u.Email, u.FirstName, u.LastName, u.LastLoginAt
          ");
          
          // Flight statistics view
          await context.Database.ExecuteSqlRawAsync(@"
              CREATE VIEW IF NOT EXISTS FlightStatisticsView AS
              SELECT 
                  DATE(f.ScheduledDepartureTime) as FlightDate,
                  f.AirlineCode,
                  f.Status,
                  COUNT(*) as FlightCount,
                  AVG(f.DelayMinutes) as AverageDelayMinutes,
                  MAX(f.DelayMinutes) as MaxDelayMinutes,
                  COUNT(CASE WHEN f.DelayMinutes > 0 THEN 1 END) as DelayedFlightCount
              FROM Flights f
              WHERE f.IsDeleted = 0
              GROUP BY DATE(f.ScheduledDepartureTime), f.AirlineCode, f.Status
          ");
      }
  }
  ```

### 15.3.3 Database Maintenance Procedures
- [ ] Create `Data/DatabaseMaintenance.cs`:
  ```csharp
  public static class DatabaseMaintenance
  {
      public static async Task RunMaintenanceAsync(FlightDbContext context)
      {
          // Vacuum database (SQLite specific)
          await context.Database.ExecuteSqlRawAsync("VACUUM;");
          
          // Analyze database statistics
          await context.Database.ExecuteSqlRawAsync("ANALYZE;");
          
          // Clean up old soft-deleted records (older than 30 days)
          var cutoffDate = DateTime.UtcNow.AddDays(-30);
          
          var oldFlights = await context.Flights
              .IgnoreQueryFilters()
              .Where(f => f.IsDeleted && f.DeletedAt < cutoffDate)
              .ToListAsync();
          
          context.Flights.RemoveRange(oldFlights);
          
          var oldUsers = await context.Users
              .IgnoreQueryFilters()
              .Where(u => u.IsDeleted && u.DeletedAt < cutoffDate)
              .ToListAsync();
          
          context.Users.RemoveRange(oldUsers);
          
          await context.SaveChangesAsync();
      }
      
      public static async Task<DatabaseHealthReport> GetHealthReportAsync(FlightDbContext context)
      {
          var report = new DatabaseHealthReport();
          
          // Table sizes
          report.FlightCount = await context.Flights.CountAsync();
          report.UserCount = await context.Users.CountAsync();
          report.RoleCount = await context.Roles.CountAsync();
          
          // Soft deleted counts
          report.SoftDeletedFlights = await context.Flights.IgnoreQueryFilters().CountAsync(f => f.IsDeleted);
          report.SoftDeletedUsers = await context.Users.IgnoreQueryFilters().CountAsync(u => u.IsDeleted);
          
          // Performance metrics
          var stopwatch = Stopwatch.StartNew();
          await context.Flights.FirstOrDefaultAsync();
          report.SimpleQueryLatencyMs = stopwatch.ElapsedMilliseconds;
          
          stopwatch.Restart();
          await context.Flights.Where(f => f.ScheduledDepartureTime.Date == DateTime.Today).CountAsync();
          report.ComplexQueryLatencyMs = stopwatch.ElapsedMilliseconds;
          
          return report;
      }
  }
  
  public class DatabaseHealthReport
  {
      public int FlightCount { get; set; }
      public int UserCount { get; set; }
      public int RoleCount { get; set; }
      public int SoftDeletedFlights { get; set; }
      public int SoftDeletedUsers { get; set; }
      public long SimpleQueryLatencyMs { get; set; }
      public long ComplexQueryLatencyMs { get; set; }
      public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
  }
  ```

### 15.3.4 Connection Pool and Performance Configuration
- [ ] Update `Program.cs` with production database settings:
  ```csharp
  // Enhanced database configuration for production
  builder.Services.AddDbContext<FlightDbContext>(options =>
  {
      options.UseSqlite(connectionString, sqliteOptions =>
      {
          sqliteOptions.CommandTimeout(30); // 30 second timeout
      });
      
      // Enable sensitive data logging only in development
      if (builder.Environment.IsDevelopment())
      {
          options.EnableSensitiveDataLogging();
          options.EnableDetailedErrors();
      }
      
      // Configure query tracking behavior
      options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  });
  
  // Add connection pooling for better performance
  builder.Services.AddDbContextPool<FlightDbContext>(options =>
      options.UseSqlite(connectionString), 
      poolSize: 128); // Maximum 128 connections in pool
  ```

### 15.3.5 Database Migration Validation
- [ ] Verify all indexes are created and being used effectively
- [ ] Test database views return correct data
- [ ] Validate foreign key constraints are working
- [ ] Run performance tests on complex queries
- [ ] Test database maintenance procedures
- [ ] Verify connection pooling is working correctly
- [ ] Run vacuum and analyze operations
- [ ] Test database backup and restore procedures

## Step 16: Health Monitoring

### 16.1 Health Checks
```cmd
cd src/FlightBoard.Api
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

### 16.2 Monitoring Implementation
- [ ] Add health check endpoints
- [ ] Monitor database connectivity
- [ ] Check external dependencies
- [ ] Add health check UI

### 16.3 Application Insights
- [ ] Add Application Insights to backend
- [ ] Configure frontend telemetry
- [ ] Set up custom metrics and events

## Step 17: API Documentation

### 17.1 Swagger Enhancement
```cmd
cd src/FlightBoard.Api
dotnet add package Swashbuckle.AspNetCore.Annotations
```

### 17.2 Documentation
- [ ] Add comprehensive API documentation
- [ ] Include request/response examples
- [ ] Document authentication requirements
- [ ] Add API versioning support

## Step 18: Security Hardening

### 18.1 Data Protection
- [ ] Implement data encryption at rest
- [ ] Add HTTPS enforcement
- [ ] Configure secure headers
- [ ] Implement CSRF protection

### 18.2 Security Testing
- [ ] Add security headers validation
- [ ] Test for common vulnerabilities
- [ ] Implement input sanitization
- [ ] Add rate limiting

## Step 19: Advanced Features

### 19.1 File Operations
- [ ] Add CSV export functionality
- [ ] Implement bulk flight import
- [ ] Add file validation and error handling

### 19.2 Notifications
- [ ] Implement email notifications for critical events
- [ ] Add system alerts for administrators
- [ ] Create notification preferences

### 19.3 Audit Logging
- [ ] Track all data modifications
- [ ] Log user actions
- [ ] Implement audit trail viewer

## Step 19.4: Advanced Audit and Logging Database Schema
```cmd
cd src/FlightBoard.Api
dotnet ef migrations add AddAuditLoggingSystem -o Data/Migrations
dotnet ef database update
```

### 19.4.1 Create Comprehensive Audit Entities
- [ ] Create `Models/AuditLog.cs`:
  ```csharp
  public class AuditLog : BaseEntity
  {
      [Required]
      [StringLength(50)]
      public string EntityName { get; set; } // Flight, User, Role, etc.
      
      [Required]
      public string EntityId { get; set; } // ID of the affected entity
      
      [Required]
      [StringLength(20)]
      public string Action { get; set; } // Create, Update, Delete, Login, Logout
      
      public string OldValues { get; set; } // JSON of old values
      public string NewValues { get; set; } // JSON of new values
      
      [Required]
      public int UserId { get; set; } // User who performed the action
      
      [Required]
      [StringLength(45)]
      public string IpAddress { get; set; }
      
      [StringLength(500)]
      public string UserAgent { get; set; }
      
      [StringLength(100)]
      public string SessionId { get; set; }
      
      public DateTime Timestamp { get; set; } = DateTime.UtcNow;
      
      // Navigation properties
      public virtual User User { get; set; }
  }
  ```

- [ ] Create `Models/SystemLog.cs`:
  ```csharp
  public class SystemLog : BaseEntity
  {
      [Required]
      [StringLength(20)]
      public string Level { get; set; } // Debug, Info, Warning, Error, Critical
      
      [Required]
      [StringLength(100)]
      public string Category { get; set; } // Database, Authentication, Flight, System
      
      [Required]
      public string Message { get; set; }
      
      public string Exception { get; set; } // Stack trace if applicable
      
      [StringLength(100)]
      public string Source { get; set; } // Method or class that generated the log
      
      [StringLength(45)]
      public string IpAddress { get; set; }
      
      public int? UserId { get; set; }
      
      [StringLength(100)]
      public string CorrelationId { get; set; } // For tracking related operations
      
      public DateTime Timestamp { get; set; } = DateTime.UtcNow;
      
      public string Properties { get; set; } // JSON for additional context
      
      // Navigation properties
      public virtual User User { get; set; }
  }
  ```

- [ ] Create `Models/PerformanceLog.cs`:
  ```csharp
  public class PerformanceLog : BaseEntity
  {
      [Required]
      [StringLength(200)]
      public string OperationName { get; set; }
      
      [Required]
      public long DurationMs { get; set; }
      
      [StringLength(50)]
      public string Category { get; set; } // Database, API, SignalR, etc.
      
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      
      public int? UserId { get; set; }
      
      [StringLength(100)]
      public string CorrelationId { get; set; }
      
      public string Parameters { get; set; } // JSON of operation parameters
      
      public bool IsSuccess { get; set; } = true;
      
      [StringLength(500)]
      public string ErrorMessage { get; set; }
      
      // Navigation properties
      public virtual User User { get; set; }
  }
  ```

### 19.4.2 Update DbContext for Audit System
- [ ] Add audit entities to `FlightDbContext.cs`:
  ```csharp
  public DbSet<AuditLog> AuditLogs { get; set; }
  public DbSet<SystemLog> SystemLogs { get; set; }
  public DbSet<PerformanceLog> PerformanceLogs { get; set; }
  
  // Add to OnModelCreating method
  ConfigureAuditEntities(modelBuilder);
  
  private void ConfigureAuditEntities(ModelBuilder modelBuilder)
  {
      // AuditLog configuration
      modelBuilder.Entity<AuditLog>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.EntityName).HasDatabaseName("IX_AuditLog_EntityName");
          entity.HasIndex(e => e.EntityId).HasDatabaseName("IX_AuditLog_EntityId");
          entity.HasIndex(e => e.Action).HasDatabaseName("IX_AuditLog_Action");
          entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditLog_UserId");
          entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLog_Timestamp");
          entity.HasIndex(e => new { e.EntityName, e.EntityId, e.Timestamp }).HasDatabaseName("IX_AuditLog_Entity_Time");
          
          entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
      });
      
      // SystemLog configuration
      modelBuilder.Entity<SystemLog>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.Level).HasDatabaseName("IX_SystemLog_Level");
          entity.HasIndex(e => e.Category).HasDatabaseName("IX_SystemLog_Category");
          entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_SystemLog_Timestamp");
          entity.HasIndex(e => e.CorrelationId).HasDatabaseName("IX_SystemLog_CorrelationId");
          entity.HasIndex(e => new { e.Level, e.Category, e.Timestamp }).HasDatabaseName("IX_SystemLog_Level_Category_Time");
          
          entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);
      });
      
      // PerformanceLog configuration
      modelBuilder.Entity<PerformanceLog>(entity =>
      {
          entity.HasKey(e => e.Id);
          entity.HasIndex(e => e.OperationName).HasDatabaseName("IX_PerformanceLog_Operation");
          entity.HasIndex(e => e.Category).HasDatabaseName("IX_PerformanceLog_Category");
          entity.HasIndex(e => e.StartTime).HasDatabaseName("IX_PerformanceLog_StartTime");
          entity.HasIndex(e => e.DurationMs).HasDatabaseName("IX_PerformanceLog_Duration");
          entity.HasIndex(e => e.CorrelationId).HasDatabaseName("IX_PerformanceLog_CorrelationId");
          entity.HasIndex(e => new { e.Category, e.StartTime }).HasDatabaseName("IX_PerformanceLog_Category_Time");
          
          entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
      });
  }
  ```

### 19.4.3 Implement Automatic Audit Tracking
- [ ] Override `SaveChanges` in `FlightDbContext.cs` for audit tracking:
  ```csharp
  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
      var auditEntries = new List<AuditLog>();
      var currentUserId = GetCurrentUserId(); // Implement this method
      
      // Track changes before saving
      foreach (var entry in ChangeTracker.Entries())
      {
          if (entry.Entity is AuditLog || entry.Entity is SystemLog || entry.Entity is PerformanceLog)
              continue; // Don't audit the audit logs
          
          if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
          {
              var auditLog = new AuditLog
              {
                  EntityName = entry.Entity.GetType().Name,
                  EntityId = GetEntityId(entry),
                  Action = entry.State.ToString(),
                  UserId = currentUserId,
                  IpAddress = GetCurrentIpAddress(), // Implement this method
                  UserAgent = GetCurrentUserAgent(), // Implement this method
                  SessionId = GetCurrentSessionId(), // Implement this method
                  Timestamp = DateTime.UtcNow
              };
              
              if (entry.State == EntityState.Modified)
              {
                  auditLog.OldValues = JsonSerializer.Serialize(GetOriginalValues(entry));
                  auditLog.NewValues = JsonSerializer.Serialize(GetCurrentValues(entry));
              }
              else if (entry.State == EntityState.Added)
              {
                  auditLog.NewValues = JsonSerializer.Serialize(GetCurrentValues(entry));
              }
              else if (entry.State == EntityState.Deleted)
              {
                  auditLog.OldValues = JsonSerializer.Serialize(GetCurrentValues(entry));
              }
              
              auditEntries.Add(auditLog);
          }
      }
      
      // Save changes first
      UpdateTimestamps();
      var result = await base.SaveChangesAsync(cancellationToken);
      
      // Then save audit logs
      if (auditEntries.Any())
      {
          AuditLogs.AddRange(auditEntries);
          await base.SaveChangesAsync(cancellationToken);
      }
      
      return result;
  }
  ```

### 19.4.4 Create Audit Query Services
- [ ] Create `Services/AuditService.cs`:
  ```csharp
  public interface IAuditService
  {
      Task<IEnumerable<AuditLog>> GetEntityAuditHistoryAsync(string entityName, string entityId);
      Task<IEnumerable<AuditLog>> GetUserActivityAsync(int userId, DateTime? fromDate = null);
      Task LogSystemEventAsync(string level, string category, string message, Exception exception = null);
      Task LogPerformanceAsync(string operationName, long durationMs, bool isSuccess = true, string errorMessage = null);
  }
  
  public class AuditService : IAuditService
  {
      private readonly FlightDbContext _context;
      private readonly IHttpContextAccessor _httpContextAccessor;
      
      public AuditService(FlightDbContext context, IHttpContextAccessor httpContextAccessor)
      {
          _context = context;
          _httpContextAccessor = httpContextAccessor;
      }
      
      public async Task<IEnumerable<AuditLog>> GetEntityAuditHistoryAsync(string entityName, string entityId)
      {
          return await _context.AuditLogs
              .Where(a => a.EntityName == entityName && a.EntityId == entityId)
              .Include(a => a.User)
              .OrderByDescending(a => a.Timestamp)
              .ToListAsync();
      }
      
      public async Task<IEnumerable<AuditLog>> GetUserActivityAsync(int userId, DateTime? fromDate = null)
      {
          var query = _context.AuditLogs.Where(a => a.UserId == userId);
          
          if (fromDate.HasValue)
              query = query.Where(a => a.Timestamp >= fromDate.Value);
          
          return await query
              .OrderByDescending(a => a.Timestamp)
              .Take(1000) // Limit to most recent 1000 actions
              .ToListAsync();
      }
      
      public async Task LogSystemEventAsync(string level, string category, string message, Exception exception = null)
      {
          var systemLog = new SystemLog
          {
              Level = level,
              Category = category,
              Message = message,
              Exception = exception?.ToString(),
              Source = GetCallingMethod(),
              IpAddress = GetCurrentIpAddress(),
              UserId = GetCurrentUserId(),
              CorrelationId = GetCorrelationId(),
              Timestamp = DateTime.UtcNow
          };
          
          _context.SystemLogs.Add(systemLog);
          await _context.SaveChangesAsync();
      }
      
      public async Task LogPerformanceAsync(string operationName, long durationMs, bool isSuccess = true, string errorMessage = null)
      {
          var performanceLog = new PerformanceLog
          {
              OperationName = operationName,
              DurationMs = durationMs,
              Category = DetermineCategory(operationName),
              StartTime = DateTime.UtcNow.AddMilliseconds(-durationMs),
              EndTime = DateTime.UtcNow,
              UserId = GetCurrentUserId(),
              CorrelationId = GetCorrelationId(),
              IsSuccess = isSuccess,
              ErrorMessage = errorMessage
          };
          
          _context.PerformanceLogs.Add(performanceLog);
          await _context.SaveChangesAsync();
      }
      
      // Helper methods implementation...
  }
  ```

### 19.4.5 Audit Database Cleanup and Archival
- [ ] Create `Services/AuditMaintenanceService.cs`:
  ```csharp
  public class AuditMaintenanceService
  {
      private readonly FlightDbContext _context;
      
      public async Task ArchiveOldAuditLogsAsync()
      {
          var cutoffDate = DateTime.UtcNow.AddMonths(-6); // Archive logs older than 6 months
          
          // Move old audit logs to archive table or external storage
          var oldAuditLogs = await _context.AuditLogs
              .Where(a => a.Timestamp < cutoffDate)
              .ToListAsync();
          
          // Implementation for archiving (could be to file, cloud storage, etc.)
          await ArchiveLogsToStorage(oldAuditLogs);
          
          // Remove from active table
          _context.AuditLogs.RemoveRange(oldAuditLogs);
          await _context.SaveChangesAsync();
      }
      
      public async Task CleanupSystemLogsAsync()
      {
          var cutoffDate = DateTime.UtcNow.AddDays(-30); // Keep system logs for 30 days
          
          var oldSystemLogs = await _context.SystemLogs
              .Where(s => s.Timestamp < cutoffDate && s.Level != "Critical")
              .ToListAsync();
          
          _context.SystemLogs.RemoveRange(oldSystemLogs);
          await _context.SaveChangesAsync();
      }
  }
  ```

## Step 19.4: Database Schema for Audit Logging
```cmd
cd src/FlightBoard.Api
dotnet ef migrations add AddAuditLogging
dotnet ef database update
```

### Database Changes:
- [ ] Create `AuditLogs` table with LogID, UserId, Action, TableName, OldValues, NewValues, Timestamp
- [ ] Add triggers or interceptors for automatic audit logging
- [ ] Create indexes on UserId and Timestamp for efficient querying
- [ ] Add data retention policies for audit logs
- [ ] Test audit logging functionality with sample operations

## Step 20: Deployment Preparation

### 20.1 Containerization
```cmd
# Add Dockerfile to API project
# Add docker-compose for local development
```

### 20.2 CI/CD Pipeline
- [ ] Set up GitHub Actions
- [ ] Configure automated testing
- [ ] Add deployment scripts
- [ ] Set up environment-specific configurations

### 20.3 Production Readiness
- [ ] Configure production database
- [ ] Set up load balancing
- [ ] Implement backup strategies
- [ ] Add monitoring and alerting

## Step 21: Testing and Validation

### 21.1 Comprehensive Testing
- [ ] End-to-end testing with Playwright
- [ ] Load testing with Apache JMeter
- [ ] Security testing
- [ ] Accessibility testing

### 21.2 User Acceptance Testing
- [ ] Create test scenarios
- [ ] Validate business requirements
- [ ] Performance validation
- [ ] Security validation

## Step 22: Documentation and Training

### 22.1 User Documentation
- [ ] Create user manuals
- [ ] Record demonstration videos
- [ ] Write troubleshooting guides

### 22.2 Technical Documentation
- [ ] Architecture documentation
- [ ] Deployment guides
- [ ] API documentation
- [ ] Maintenance procedures

## Quality Gates

After each step, ensure:
- [ ] All tests pass
- [ ] Code review completed
- [ ] Documentation updated
- [ ] Feature works end-to-end
- [ ] No breaking changes introduced

## Development Best Practices

### Code Quality
- Follow SOLID principles
- Implement proper error handling
- Write meaningful tests
- Use consistent naming conventions

### Git Workflow
- Create feature branches for each step
- Write descriptive commit messages
- Use pull requests for code review
- Tag releases appropriately

### Testing Strategy
- Write tests before implementing features (TDD)
- Maintain high code coverage (>80%)
- Test both happy path and error scenarios
- Include integration and e2e tests
#### 2.1 Core API Development
- [ ] Implement `GET /api/v1/flights` to retrieve the current list of flights.
- [ ] Implement `POST /api/v1/flights` to add a new flight after validating the input.
- [ ] Implement `PUT /api/v1/flights/{id}` to update flight information.
- [ ] Implement `DELETE /api/v1/flights/{id}` to remove a flight after confirmation.
- [ ] Implement `GET /api/v1/flights/search` to retrieve flights based on search criteria.

#### 2.2 Authentication & Authorization
- [ ] Implement JWT-based authentication system.
- [ ] Create Users, Roles, and UserRoles tables.
- [ ] Implement role-based access control (RBAC) middleware.
- [ ] Add multi-factor authentication for admin accounts.
- [ ] Implement session management with timeout.

#### 2.3 Business Logic Implementation
- [ ] Create **FlightManager** for orchestrating flight operations.
- [ ] Implement **FlightEngine** for business logic and status calculations.
- [ ] Develop **FlightAccessor** for database operations.
- [ ] Add dynamic flight status calculation based on departure times.
- [ ] Implement comprehensive server-side validation.

#### 2.4 Performance & Caching
- [ ] Implement Redis caching for frequently accessed data.
- [ ] Add database indexing for optimized queries.
- [ ] Implement query result caching.
- [ ] Add rate limiting and throttling middleware.

#### 2.5 Monitoring & Health Checks
- [ ] Implement `/health` endpoint with dependency checks.
- [ ] Add structured logging with Serilog.
- [ ] Implement performance metrics collection.
- [ ] Set up error tracking and alerting.

### Phase 3: Frontend Development
#### 3.1 Backoffice Application
- [ ] Create administrative dashboard layout.
- [ ] Implement flight management forms (add, edit, delete).
- [ ] Add user management interface for admins.
- [ ] Implement audit log viewer.
- [ ] Add role-based UI component visibility.

#### 3.2 Consumer Application
- [ ] Create public flight board interface.
- [ ] Implement real-time flight status display.
- [ ] Add search and filter functionality.
- [ ] Implement responsive design for mobile devices.
- [ ] Add offline capability with service workers.

#### 3.3 Shared Frontend Infrastructure
- [ ] Implement authentication components and guards.
- [ ] Set up TanStack Query for server state management.
- [ ] Configure Redux Toolkit for UI state management.
- [ ] Implement SignalR client connection management.
- [ ] Add error boundaries for graceful error handling.

### Phase 4: Security Implementation
#### 4.1 Data Protection
- [ ] Implement data encryption at rest (AES-256).
- [ ] Ensure TLS encryption for data in transit.
- [ ] Add input sanitization and validation.
- [ ] Implement CSRF protection.

#### 4.2 Access Control
- [ ] Add IP-based access restrictions for sensitive operations.
- [ ] Implement session security measures.
- [ ] Add comprehensive audit logging.
- [ ] Ensure GDPR compliance with data anonymization.

### Phase 5: Testing Implementation
#### 5.1 Backend Testing
- [ ] Write unit tests for FlightManager, FlightEngine, and FlightAccessor.
- [ ] Implement integration tests for API endpoints.
- [ ] Add database integration tests.
- [ ] Create load tests using Apache JMeter.
- [ ] Implement security testing with OWASP ZAP.

#### 5.2 Frontend Testing
- [ ] Write unit tests for React components.
- [ ] Implement integration tests for user workflows.
- [ ] Add end-to-end tests with Playwright or Cypress.
- [ ] Perform cross-browser compatibility testing.
- [ ] Implement accessibility testing with axe-core.

### Phase 6: DevOps & Deployment
#### 6.1 CI/CD Pipeline
- [ ] Set up GitHub Actions or Azure DevOps pipelines.
- [ ] Implement automated testing in CI pipeline.
- [ ] Add security scanning and dependency checks.
- [ ] Configure automated deployment to staging and production.

#### 6.2 Environment Configuration
- [ ] Set up separate configurations for dev, staging, and production.
- [ ] Implement feature flags for gradual rollouts.
- [ ] Configure environment-specific secrets and settings.
- [ ] Set up database migration automation.

#### 6.3 Backup & Recovery
- [ ] Implement automated daily backups.
- [ ] Set up cross-region backup replication.
- [ ] Create disaster recovery procedures.
- [ ] Test backup and recovery processes monthly.

### Phase 7: Operational Excellence
#### 7.1 Monitoring & Alerting
- [ ] Configure Azure Monitor dashboards.
- [ ] Set up alerting for critical metrics (response time, error rate, uptime).
- [ ] Implement log aggregation and analysis.
- [ ] Create performance monitoring reports.

#### 7.2 Scalability Preparation
- [ ] Implement load balancing configuration.
- [ ] Prepare for horizontal scaling.
- [ ] Set up CDN for static asset delivery.
- [ ] Optimize database queries and indexing.

### Phase 8: Integration & Extensions
#### 8.1 Third-party Integrations
- [ ] Create framework for external API integrations.
- [ ] Implement file upload/download capabilities.
- [ ] Add email and SMS notification services.
- [ ] Prepare for weather data integration.

#### 8.2 Advanced Features
- [ ] Implement advanced search with full-text capabilities.
- [ ] Add data export functionality (CSV, Excel).
- [ ] Create analytics and reporting features.
- [ ] Implement multi-language support preparation.

### Phase 9: Documentation & Training
#### 9.1 Technical Documentation
- [ ] Create comprehensive API documentation with Swagger.
- [ ] Write deployment and operations guides.
- [ ] Document architecture decisions and trade-offs.
- [ ] Create troubleshooting guides.

#### 9.2 User Documentation
- [ ] Create user manuals for both applications.
- [ ] Develop admin training materials.
- [ ] Write system administration guides.
- [ ] Create video tutorials for key workflows.

### Phase 10: Launch Preparation
#### 10.1 Performance Validation
- [ ] Conduct load testing to validate SLA requirements.
- [ ] Perform security penetration testing.
- [ ] Validate disaster recovery procedures.
- [ ] Test all monitoring and alerting systems.

#### 10.2 Go-Live Activities
- [ ] Deploy to production environment.
- [ ] Monitor system health during initial launch.
- [ ] Validate real-time functionality with actual users.
- [ ] Conduct post-launch performance review.

## Quality Gates
Each phase should meet the following criteria before proceeding:
- [ ] All tests passing with minimum 80% code coverage.
- [ ] Security scan with no high-severity vulnerabilities.
- [ ] Performance requirements met (99.9% uptime, <200ms response).
- [ ] Documentation complete and reviewed.
- [ ] Stakeholder approval obtained.
