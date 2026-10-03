using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public DbSet<StatusChange> StatusChanges => Set<StatusChange>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // We always save times in UTC, but SQL Server's datetime2 column doesn't remember that.
        // Mark every DateTime read back as UTC, so the JSON ends in "Z" and browsers show the right local time.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
        {
        }
    }

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

            // One application has many status changes. Deleting the application deletes its history.
            entity.HasMany(e => e.StatusHistory)
                .WithOne()
                .HasForeignKey(s => s.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StatusChange>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
        });
    }
}
