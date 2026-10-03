using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Data;

/// <summary>
/// The EF Core database context. Each DbSet becomes a table in SQL Server.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<JobApplication>(entity =>
        {
            entity.Property(e => e.Company).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.JobLink).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(4000);

            // Store the status as text (e.g. "Interview") so the table is easy to read.
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
        });
    }
}
