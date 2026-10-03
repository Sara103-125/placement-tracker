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
        var application = new JobApplication();
        CopyRequestToEntity(request, application);
        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = application.CreatedAt;

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

        CopyRequestToEntity(request, application);
        application.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return application;
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
