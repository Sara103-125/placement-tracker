using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Auth;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Controllers;

/// <summary>
/// Interviews, online tests and calls scheduled for one application.
/// The URL includes the application id, because an event always belongs to an application.
/// </summary>
[ApiController]
[Authorize]
[Route("api/applications/{applicationId}/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>True if the application exists and belongs to the logged-in user.</summary>
    private Task<bool> OwnsApplication(int applicationId)
    {
        var userId = User.GetUserId();
        return _db.JobApplications.AnyAsync(a => a.Id == applicationId && a.UserId == userId);
    }

    // GET api/applications/5/events
    [HttpGet]
    public async Task<ActionResult<List<ApplicationEvent>>> GetAll(int applicationId)
    {
        if (!await OwnsApplication(applicationId))
        {
            return NotFound();
        }

        return await _db.ApplicationEvents
            .Where(e => e.JobApplicationId == applicationId)
            .OrderBy(e => e.StartsAt)
            .ToListAsync();
    }

    // POST api/applications/5/events
    [HttpPost]
    public async Task<ActionResult<ApplicationEvent>> Create(int applicationId, EventRequest request)
    {
        if (!await OwnsApplication(applicationId))
        {
            return NotFound();
        }

        var applicationEvent = new ApplicationEvent
        {
            JobApplicationId = applicationId,
            Title = request.Title.Trim(),
            Type = request.Type,
            StartsAt = request.StartsAt!.Value.ToUniversalTime(),
            Notes = request.Notes,
        };

        _db.ApplicationEvents.Add(applicationEvent);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { applicationId }, applicationEvent);
    }

    // DELETE api/applications/5/events/12
    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Delete(int applicationId, int eventId)
    {
        if (!await OwnsApplication(applicationId))
        {
            return NotFound();
        }

        var applicationEvent = await _db.ApplicationEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.JobApplicationId == applicationId);
        if (applicationEvent == null)
        {
            return NotFound();
        }

        _db.ApplicationEvents.Remove(applicationEvent);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
