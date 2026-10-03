using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Tests;

public class DashboardControllerTests
{
    private static JobApplication NewApplication(string company, ApplicationStatus status, DateOnly? deadline = null) => new()
    {
        Company = company,
        Role = "Intern",
        Status = status,
        Deadline = deadline,
    };

    [Fact]
    public async Task GetSummary_CountsEveryStatus_IncludingZeros()
    {
        using var db = TestDb.Create();
        db.JobApplications.AddRange(
            NewApplication("Google", ApplicationStatus.Applied),
            NewApplication("Amazon", ApplicationStatus.Applied),
            NewApplication("Microsoft", ApplicationStatus.Offer));
        await db.SaveChangesAsync();

        var summary = (await new DashboardController(db).GetSummary()).Value!;

        Assert.Equal(3, summary.Total);
        Assert.Equal(2, summary.StatusCounts[ApplicationStatus.Applied]);
        Assert.Equal(1, summary.StatusCounts[ApplicationStatus.Offer]);
        Assert.Equal(0, summary.StatusCounts[ApplicationStatus.Rejected]);
        Assert.Equal(6, summary.StatusCounts.Count); // every status is present
    }

    [Fact]
    public async Task GetSummary_UpcomingDeadlines_OnlyIncludesTodayToSevenDaysAhead_SoonestFirst()
    {
        using var db = TestDb.Create();
        var today = DateOnly.FromDateTime(DateTime.Today);
        db.JobApplications.AddRange(
            NewApplication("Yesterday", ApplicationStatus.Applied, today.AddDays(-1)),
            NewApplication("In7Days", ApplicationStatus.Applied, today.AddDays(7)),
            NewApplication("Today", ApplicationStatus.Applied, today),
            NewApplication("In8Days", ApplicationStatus.Applied, today.AddDays(8)),
            NewApplication("NoDeadline", ApplicationStatus.Applied));
        await db.SaveChangesAsync();

        var summary = (await new DashboardController(db).GetSummary()).Value!;

        Assert.Equal(new[] { "Today", "In7Days" }, summary.UpcomingDeadlines.Select(a => a.Company));
    }
}
