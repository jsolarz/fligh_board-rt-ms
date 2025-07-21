using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Models;
using System.Linq.Expressions;

namespace FlightBoard.Api.Data;

/// <summary>
/// Database context for flight board system with SQLite support
/// </summary>
public class FlightDbContext : DbContext
{
    public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Flight entities table
    /// </summary>
    public DbSet<Flight> Flights { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Flight entity
        ConfigureFlightEntity(modelBuilder);

        // Configure global query filters for soft delete
        ConfigureSoftDeleteFilters(modelBuilder);
    }

    /// <summary>
    /// Configure Flight entity with indexes and constraints
    /// </summary>
    private void ConfigureFlightEntity(ModelBuilder modelBuilder)
    {
        var flightEntity = modelBuilder.Entity<Flight>();

        // Table configuration
        flightEntity.ToTable("Flights");

        // Indexes for performance optimization
        flightEntity.HasIndex(f => f.FlightNumber)
            .HasDatabaseName("IX_Flights_FlightNumber");

        flightEntity.HasIndex(f => f.Airline)
            .HasDatabaseName("IX_Flights_Airline");

        flightEntity.HasIndex(f => f.Origin)
            .HasDatabaseName("IX_Flights_Origin");

        flightEntity.HasIndex(f => f.Destination)
            .HasDatabaseName("IX_Flights_Destination");

        flightEntity.HasIndex(f => f.Status)
            .HasDatabaseName("IX_Flights_Status");

        flightEntity.HasIndex(f => f.Type)
            .HasDatabaseName("IX_Flights_Type");

        flightEntity.HasIndex(f => f.ScheduledDeparture)
            .HasDatabaseName("IX_Flights_ScheduledDeparture");

        flightEntity.HasIndex(f => f.ScheduledArrival)
            .HasDatabaseName("IX_Flights_ScheduledArrival");

        // Composite indexes for common queries
        flightEntity.HasIndex(f => new { f.FlightNumber, f.ScheduledDeparture })
            .HasDatabaseName("IX_Flights_FlightNumber_ScheduledDeparture");

        flightEntity.HasIndex(f => new { f.Type, f.Status })
            .HasDatabaseName("IX_Flights_Type_Status");

        flightEntity.HasIndex(f => new { f.Origin, f.Type })
            .HasDatabaseName("IX_Flights_Origin_Type");

        flightEntity.HasIndex(f => new { f.Destination, f.Type })
            .HasDatabaseName("IX_Flights_Destination_Type");

        // Index for soft delete
        flightEntity.HasIndex(f => f.IsDeleted)
            .HasDatabaseName("IX_Flights_IsDeleted");

        // Enum configurations
        flightEntity.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        flightEntity.Property(f => f.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Property configurations
        flightEntity.Property(f => f.CreatedAt)
            .HasDefaultValueSql("datetime('now')")
            .ValueGeneratedOnAdd();

        flightEntity.Property(f => f.UpdatedAt)
            .HasDefaultValueSql("datetime('now')")
            .ValueGeneratedOnAddOrUpdate();

        // Computed properties (read-only)
        flightEntity.Ignore(f => f.IsDelayed);
        flightEntity.Ignore(f => f.EstimatedDeparture);
        flightEntity.Ignore(f => f.EstimatedArrival);
    }

    /// <summary>
    /// Configure global query filters for soft delete functionality
    /// </summary>
    private void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                    Expression.Constant(false));
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically update audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update audit fields
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update audit fields (CreatedAt, UpdatedAt) for tracked entities
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    // Prevent modification of CreatedAt
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}
