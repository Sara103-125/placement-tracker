using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlacementTracker.Api.Auth;
using PlacementTracker.Api.Controllers;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Tests;

public class AuthControllerTests
{
    private static AuthController NewController(Api.Data.AppDbContext db)
    {
        var settings = Options.Create(new JwtSettings
        {
            Key = "test-key-that-is-definitely-at-least-32-characters",
            Issuer = "test",
            Audience = "test",
        });
        return new AuthController(db, new TokenService(settings));
    }

    private static AuthRequest Credentials(string email = "Sam@Uni.ac.uk", string password = "password123") =>
        new() { Email = email, Password = password };

    [Fact]
    public async Task Register_CreatesUser_WithHashedPassword_AndReturnsToken()
    {
        using var db = TestDb.Create();

        var result = await NewController(db).Register(Credentials());

        Assert.False(string.IsNullOrEmpty(result.Value!.Token));
        var user = Assert.Single(db.Users);
        Assert.Equal("sam@uni.ac.uk", user.Email);          // stored lower-case
        Assert.NotEqual("password123", user.PasswordHash);  // never the plain password
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyUsed()
    {
        using var db = TestDb.Create();
        await NewController(db).Register(Credentials());

        var result = await NewController(db).Register(Credentials(email: "sam@uni.ac.uk"));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_Succeeds_WithCorrectPassword_AndFails_WithWrongOne()
    {
        using var db = TestDb.Create();
        await NewController(db).Register(Credentials());

        var good = await NewController(db).Login(Credentials());
        var bad = await NewController(db).Login(Credentials(password: "wrong-password"));

        Assert.False(string.IsNullOrEmpty(good.Value!.Token));
        Assert.IsType<UnauthorizedObjectResult>(bad.Result);
    }

    [Fact]
    public async Task FirstUser_AdoptsApplicationsCreatedBeforeAccountsExisted()
    {
        using var db = TestDb.Create();
        db.JobApplications.Add(new JobApplication { Company = "Old Co", Role = "Intern" }); // no owner
        await db.SaveChangesAsync();

        await NewController(db).Register(Credentials());

        Assert.Equal(db.Users.Single().Id, db.JobApplications.Single().UserId);
    }
}
