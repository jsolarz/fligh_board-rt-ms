using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;

namespace FlightBoard.Tests;

public abstract class BaseTestClass : IDisposable
{
    protected FlightDbContext Context { get; private set; }

    protected BaseTestClass()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new FlightDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
