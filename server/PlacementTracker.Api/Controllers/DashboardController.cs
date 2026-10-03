using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Auth;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/dashboard/summary
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummary>> GetSummary()
    {
        var userId = User.GetUserId();
        var myApplications = _db.JobApplications.Where(a => a.UserId == userId);

        // Ask the database for "status -> count" for the statuses that have rows.
        var counts = await myApplications
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        // Start every status at 0 so the dashboard always shows all six.
        var statusCounts = new Dictionary<ApplicationStatus, int>();
        foreach (var status in Enum.GetValues<ApplicationStatus>())
        {
            statusCounts[status] = counts.GetValueOrDefault(status);
        }

        var funnel = await BuildFunnel(userId);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var nextWeek = today.AddDays(7);

        var upcoming = await myApplications
            .Where(a => a.Deadline >= today && a.Deadline <= nextWeek)
            .OrderBy(a => a.Deadline)
            .ToListAsync();

        // Interviews, tests and calls in the next 7 days, with the company they belong to.
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);
        var upcomingEvents = await (
                from e in _db.ApplicationEvents
                join a in myApplications on e.JobApplicationId equals a.Id
                where e.StartsAt >= now && e.StartsAt <= weekFromNow
                orderby e.StartsAt
                select new UpcomingEvent
                {
                    Id = e.Id,
                    ApplicationId = a.Id,
                    Company = a.Company,
                    Role = a.Role,
                    Title = e.Title,
                    Type = e.Type,
                    StartsAt = e.StartsAt,
                })
            .ToListAsync();

        return new DashboardSummary
        {
            Total = counts.Values.Sum(),
            StatusCounts = statusCounts,
            Funnel = funnel,
            UpcomingDeadlines = upcoming,
            UpcomingEvents = upcomingEvents,
        };
    }

    /// <summary>
    /// The funnel answers: "of everything I applied to, how far did each one get?"
    /// It uses the status history, so an application that reached Interview and was later
    /// Rejected still counts as having reached Applied, Online Test and Interview.
    /// </summary>
    private async Task<List<FunnelStep>> BuildFunnel(int userId)
    {
        // The stages in pipeline order. Wishlist (not applied yet) and Rejected (an outcome) are not stages.
        var stages = new[]
        {
            ApplicationStatus.Applied,
            ApplicationStatus.OnlineTest,
            ApplicationStatus.Interview,
            ApplicationStatus.Offer,
        };

        var history = await (
                from s in _db.StatusChanges
                join a in _db.JobApplications on s.JobApplicationId equals a.Id
                where a.UserId == userId
                      && s.Status != ApplicationStatus.Wishlist
                      && s.Status != ApplicationStatus.Rejected
                select new { s.JobApplicationId, s.Status })
            .ToListAsync();

        // For each application, the furthest stage it ever reached (0 = Applied ... 3 = Offer).
        var furthestStage = history
            .GroupBy(s => s.JobApplicationId)
            .Select(g => g.Max(s => Array.IndexOf(stages, s.Status)))
            .ToList();

        // Reaching a later stage means it also passed the earlier ones, even if it skipped them.
        return stages
            .Select((stage, index) => new FunnelStep
            {
                Status = stage,
                Count = furthestStage.Count(furthest => furthest >= index),
            })
            .ToList();
    }
}
