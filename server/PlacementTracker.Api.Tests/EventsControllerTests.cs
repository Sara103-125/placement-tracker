using Microsoft.AspNetCore.Mvc;
using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task CreateListAndDeleteEvents()
    {
        using var db = TestDb.Create();
        var userId = TestDb.AddUser(db);
        await new ApplicationsController(db).As(userId).Create(new JobApplicationRequest { Company = "Google", Role = "Intern" });
        var appId = db.JobApplications.Single().Id;
        var controller = new EventsController(db).As(userId);

        var created = await controller.Create(appId, new EventRequest
        {
            Title = "Technical interview",
            Type = EventType.Interview,
            StartsAt = DateTime.UtcNow.AddDays(2),
        });
        var eventId = Assert.IsType<ApplicationEvent>(Assert.IsType<CreatedAtActionResult>(created.Result).Value).Id;

        Assert.Single((await controller.GetAll(appId)).Value!);
        Assert.IsType<NoContentResult>(await controller.Delete(appId, eventId));
        Assert.Empty((await controller.GetAll(appId)).Value!);
    }

    [Fact]
    public async Task Dashboard_ShowsOnlyEventsInTheNextSevenDays()
    {
        using var db = TestDb.Create();
        var userId = TestDb.AddUser(db);
        await new ApplicationsController(db).As(userId).Create(new JobApplicationRequest { Company = "Google", Role = "Intern" });
        var appId = db.JobApplications.Single().Id;
        var now = DateTime.UtcNow;
        db.ApplicationEvents.AddRange(
            new ApplicationEvent { JobApplicationId = appId, Title = "Past", StartsAt = now.AddDays(-1) },
            new ApplicationEvent { JobApplicationId = appId, Title = "In 3 days", StartsAt = now.AddDays(3) },
            new ApplicationEvent { JobApplicationId = appId, Title = "Tomorrow", StartsAt = now.AddDays(1) },
            new ApplicationEvent { JobApplicationId = appId, Title = "In 10 days", StartsAt = now.AddDays(10) });
        await db.SaveChangesAsync();

        var summary = (await new DashboardController(db).As(userId).GetSummary()).Value!;

        Assert.Equal(new[] { "Tomorrow", "In 3 days" }, summary.UpcomingEvents.Select(e => e.Title));
        Assert.All(summary.UpcomingEvents, e => Assert.Equal("Google", e.Company));
    }
}
