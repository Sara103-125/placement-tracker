namespace PlacementTracker.Api.Entities;

/// <summary>
/// One entry in an application's status history, e.g. "moved to Interview on 5 Oct".
/// A new row is added whenever an application is created or its status changes.
/// </summary>
public class StatusChange
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    public ApplicationStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
}
