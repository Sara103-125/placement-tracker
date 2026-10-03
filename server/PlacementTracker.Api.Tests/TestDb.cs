using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Entities;

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

    /// <summary>Adds a user to the database and returns their id.</summary>
    public static int AddUser(AppDbContext db, string email = "student@uni.ac.uk")
    {
        var user = new User { Email = email, PasswordHash = "x", CreatedAt = DateTime.UtcNow };
        db.Users.Add(user);
        db.SaveChanges();
        return user.Id;
    }

    /// <summary>
    /// Makes a controller behave as if this user is logged in, the same as a valid token would.
    /// </summary>
    public static T As<T>(this T controller, int userId) where T : ControllerBase
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
        };
        return controller;
    }
}
