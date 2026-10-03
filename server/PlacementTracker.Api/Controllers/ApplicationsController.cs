using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/applications?search=google&status=Interview
    [HttpGet]
    public async Task<ActionResult<List<JobApplication>>> GetAll(string? search, ApplicationStatus? status)
    {
        var query = _db.JobApplications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.Company.Contains(search) || a.Role.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query.OrderByDescending(a => a.UpdatedAt).ToListAsync();
    }

    // GET api/applications/5
    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplication>> GetById(int id)
    {
        var application = await _db.JobApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        return application;
    }

    // POST api/applications
    [HttpPost]
    public async Task<ActionResult<JobApplication>> Create(JobApplicationRequest request)
    {
        var now = DateTime.UtcNow;
        var application = new JobApplication();
        CopyRequestToEntity(request, application);
        application.CreatedAt = now;
        application.UpdatedAt = now;

        // The first entry in the history is the status it was created with.
        application.StatusHistory.Add(new StatusChange { Status = application.Status, ChangedAt = now });

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();

        // 201 Created, with a Location header pointing at the new item.
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
    }

    // PUT api/applications/5
    [HttpPut("{id}")]
    public async Task<ActionResult<JobApplication>> Update(int id, JobApplicationRequest request)
    {
        var application = await _db.JobApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var oldStatus = application.Status;
        CopyRequestToEntity(request, application);
        application.UpdatedAt = DateTime.UtcNow;
        RecordStatusChangeIfNeeded(application, oldStatus);

        await _db.SaveChangesAsync();
        return application;
    }

    // PATCH api/applications/5/status   body: { "status": "Interview" }
    // Changes only the status. Used by the Kanban board when a card is dragged to another column.
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<JobApplication>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var application = await _db.JobApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var oldStatus = application.Status;
        application.Status = request.Status;
        application.UpdatedAt = DateTime.UtcNow;
        RecordStatusChangeIfNeeded(application, oldStatus);

        await _db.SaveChangesAsync();
        return application;
    }

    // GET api/applications/5/history
    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<StatusChange>>> GetHistory(int id)
    {
        if (!await _db.JobApplications.AnyAsync(a => a.Id == id))
        {
            return NotFound();
        }

        return await _db.StatusChanges
            .Where(s => s.JobApplicationId == id)
            .OrderBy(s => s.ChangedAt)
            .ToListAsync();
    }

    // DELETE api/applications/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var application = await _db.JobApplications.FindAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        _db.JobApplications.Remove(application);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Adds a history entry, but only when the status actually changed.</summary>
    private static void RecordStatusChangeIfNeeded(JobApplication application, ApplicationStatus oldStatus)
    {
        if (application.Status != oldStatus)
        {
            application.StatusHistory.Add(new StatusChange
            {
                Status = application.Status,
                ChangedAt = application.UpdatedAt,
            });
        }
    }

    private static void CopyRequestToEntity(JobApplicationRequest request, JobApplication application)
    {
        application.Company = request.Company.Trim();
        application.Role = request.Role.Trim();
        application.Location = request.Location;
        application.JobLink = request.JobLink;
        application.DateApplied = request.DateApplied;
        application.Deadline = request.Deadline;
        application.Status = request.Status;
        application.Notes = request.Notes;
    }
}
