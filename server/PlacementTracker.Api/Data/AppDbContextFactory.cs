using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlacementTracker.Api.Data;

/// <summary>
/// Used by `dotnet ef` at design time so migrations do not need the web host running.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=PlacementTracker;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True");

        return new AppDbContext(optionsBuilder.Options);
    }
}
