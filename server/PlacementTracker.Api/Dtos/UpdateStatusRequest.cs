using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Dtos;

/// <summary>
/// Body for PATCH /api/applications/{id}/status. Used by the Kanban board,
/// which only changes the status and leaves every other field alone.
/// </summary>
public class UpdateStatusRequest
{
    public ApplicationStatus Status { get; set; }
}
