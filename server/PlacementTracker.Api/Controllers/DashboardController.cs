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
            UpcomingDeadlines = upcoming
        };
    }
}
