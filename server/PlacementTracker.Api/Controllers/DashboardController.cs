using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Controllers;

[ApiController]
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
        // Ask the database for "status -> count" for the statuses that have rows.
        var counts = await _db.JobApplications
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        // Start every status at 0 so the dashboard always shows all six.
        var statusCounts = new Dictionary<ApplicationStatus, int>();
        foreach (var status in Enum.GetValues<ApplicationStatus>())
        {
            statusCounts[status] = counts.GetValueOrDefault(status);
        }

        var funnel = await BuildFunnel();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var nextWeek = today.AddDays(7);

        var upcoming = await _db.JobApplications
            .Where(a => a.Deadline >= today && a.Deadline <= nextWeek)
            .OrderBy(a => a.Deadline)
            .ToListAsync();

        return new DashboardSummary
        {
            Total = counts.Values.Sum(),
            StatusCounts = statusCounts,
            Funnel = funnel,
            UpcomingDeadlines = upcoming
        };
    }

    /// <summary>
    /// The funnel answers: "of everything I applied to, how far did each one get?"
    /// It uses the status history, so an application that reached Interview and was later
    /// Rejected still counts as having reached Applied, Online Test and Interview.
    /// </summary>
    private async Task<List<FunnelStep>> BuildFunnel()
    {
        // The stages in pipeline order. Wishlist (not applied yet) and Rejected (an outcome) are not stages.
        var stages = new[]
        {
            ApplicationStatus.Applied,
            ApplicationStatus.OnlineTest,
            ApplicationStatus.Interview,
            ApplicationStatus.Offer,
        };

        var history = await _db.StatusChanges
            .Where(s => s.Status != ApplicationStatus.Wishlist && s.Status != ApplicationStatus.Rejected)
            .Select(s => new { s.JobApplicationId, s.Status })
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
