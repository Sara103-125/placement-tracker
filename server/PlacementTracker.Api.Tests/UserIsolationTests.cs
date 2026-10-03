using Microsoft.AspNetCore.Mvc;
using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Tests;

/// <summary>
/// The most important security rule: one user can never see or change another user's data.
/// </summary>
public class UserIsolationTests
{
    [Fact]
    public async Task UsersOnlySeeTheirOwnApplications()
    {
        using var db = TestDb.Create();
        var alice = TestDb.AddUser(db, "alice@uni.ac.uk");
        var bob = TestDb.AddUser(db, "bob@uni.ac.uk");
        await new ApplicationsController(db).As(alice).Create(new JobApplicationRequest { Company = "Google", Role = "Intern" });

        var bobsList = await new ApplicationsController(db).As(bob).GetAll(null, null);
        var bobsDashboard = (await new DashboardController(db).As(bob).GetSummary()).Value!;

        Assert.Empty(bobsList.Value!);
        Assert.Equal(0, bobsDashboard.Total);
    }

    [Fact]
    public async Task UsersCannotReadChangeOrDeleteSomeoneElsesApplication()
    {
        using var db = TestDb.Create();
        var alice = TestDb.AddUser(db, "alice@uni.ac.uk");
        var bob = TestDb.AddUser(db, "bob@uni.ac.uk");
        await new ApplicationsController(db).As(alice).Create(new JobApplicationRequest { Company = "Google", Role = "Intern" });
        var alicesId = db.JobApplications.Single().Id;
        var asBob = new ApplicationsController(db).As(bob);

        Assert.IsType<NotFoundResult>((await asBob.GetById(alicesId)).Result);
        Assert.IsType<NotFoundResult>((await asBob.Update(alicesId, new JobApplicationRequest { Company = "Hacked", Role = "x" })).Result);
        Assert.IsType<NotFoundResult>((await asBob.UpdateStatus(alicesId, new UpdateStatusRequest { Status = ApplicationStatus.Rejected })).Result);
        Assert.IsType<NotFoundResult>(await asBob.Delete(alicesId));
        Assert.IsType<NotFoundResult>((await new EventsController(db).As(bob).GetAll(alicesId)).Result);

        Assert.Equal("Google", db.JobApplications.Single().Company); // untouched
    }
}
