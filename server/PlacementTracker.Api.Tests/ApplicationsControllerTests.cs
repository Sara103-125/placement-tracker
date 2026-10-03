using Microsoft.AspNetCore.Mvc;
using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Tests;

public class ApplicationsControllerTests
{
    private static JobApplicationRequest NewRequest(string company = "Google", string role = "SWE Intern") => new()
    {
        Company = company,
        Role = role,
        Status = ApplicationStatus.Applied,
    };

    [Fact]
    public async Task Create_SavesApplication_AndReturns201()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);

        var result = await controller.Create(NewRequest(company: "  Google  "));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var application = Assert.IsType<JobApplication>(created.Value);
        Assert.Equal("Google", application.Company); // trimmed
        Assert.NotEqual(default, application.CreatedAt);
        Assert.Equal(1, db.JobApplications.Count());
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenApplicationDoesNotExist()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);
        await controller.Create(NewRequest());
        var id = db.JobApplications.Single().Id;

        var request = NewRequest();
        request.Status = ApplicationStatus.Interview;
        request.Notes = "Interview on Monday";
        var result = await controller.Update(id, request);

        Assert.Equal(ApplicationStatus.Interview, result.Value!.Status);
        Assert.Equal("Interview on Monday", db.JobApplications.Single().Notes);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenApplicationDoesNotExist()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);

        var result = await controller.Update(999, NewRequest());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_RemovesApplication()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);
        await controller.Create(NewRequest());
        var id = db.JobApplications.Single().Id;

        var result = await controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.JobApplications);
    }

    [Fact]
    public async Task GetAll_SearchMatchesCompanyOrRole()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);
        await controller.Create(NewRequest(company: "Google", role: "SWE Intern"));
        await controller.Create(NewRequest(company: "Amazon", role: "Data Intern"));
        await controller.Create(NewRequest(company: "Monzo", role: "Backend Intern"));

        var byCompany = await controller.GetAll("Goo", null);
        var byRole = await controller.GetAll("Data", null);

        Assert.Equal("Google", Assert.Single(byCompany.Value!).Company);
        Assert.Equal("Amazon", Assert.Single(byRole.Value!).Company);
    }

    [Fact]
    public async Task GetAll_FiltersByStatus()
    {
        using var db = TestDb.Create();
        var controller = new ApplicationsController(db);
        var offer = NewRequest(company: "Microsoft");
        offer.Status = ApplicationStatus.Offer;
        await controller.Create(offer);
        await controller.Create(NewRequest(company: "Google"));

        var result = await controller.GetAll(null, ApplicationStatus.Offer);

        Assert.Equal("Microsoft", Assert.Single(result.Value!).Company);
    }
}
