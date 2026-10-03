using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Dtos;
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
    public async Task GetSummary_Funnel_CountsTheFurthestStageEachApplicationReached()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);

        // A: Applied only.  B: Applied -> Interview -> Rejected.  C: Applied -> Online Test -> Interview -> Offer.
        await controller.Create(new JobApplicationRequest { Company = "A", Role = "Intern", Status = ApplicationStatus.Applied });
        await controller.Create(new JobApplicationRequest { Company = "B", Role = "Intern", Status = ApplicationStatus.Applied });
        await controller.Create(new JobApplicationRequest { Company = "C", Role = "Intern", Status = ApplicationStatus.Applied });
        var b = db.JobApplications.Single(a => a.Company == "B").Id;
        var c = db.JobApplications.Single(a => a.Company == "C").Id;
        await controller.UpdateStatus(b, new UpdateStatusRequest { Status = ApplicationStatus.Interview });
        await controller.UpdateStatus(b, new UpdateStatusRequest { Status = ApplicationStatus.Rejected });
        await controller.UpdateStatus(c, new UpdateStatusRequest { Status = ApplicationStatus.OnlineTest });
        await controller.UpdateStatus(c, new UpdateStatusRequest { Status = ApplicationStatus.Interview });
        await controller.UpdateStatus(c, new UpdateStatusRequest { Status = ApplicationStatus.Offer });

        var summary = (await new DashboardController(db).GetSummary()).Value!;
        var funnel = summary.Funnel.ToDictionary(f => f.Status, f => f.Count);

        Assert.Equal(3, funnel[ApplicationStatus.Applied]);
        Assert.Equal(2, funnel[ApplicationStatus.OnlineTest]); // B skipped it, but reached a later stage
        Assert.Equal(2, funnel[ApplicationStatus.Interview]);  // B still counts even though it was rejected
        Assert.Equal(1, funnel[ApplicationStatus.Offer]);
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
