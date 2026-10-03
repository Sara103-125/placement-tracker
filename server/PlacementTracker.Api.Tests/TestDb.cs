using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;

namespace PlacementTracker.Api.Tests;

/// <summary>
/// Creates an AppDbContext backed by EF Core's in-memory database, so tests run fast
/// and never touch the real LocalDB database. Each call gets its own empty database.
/// </summary>
public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
