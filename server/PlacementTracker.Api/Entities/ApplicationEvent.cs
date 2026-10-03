namespace PlacementTracker.Api.Entities;

/// <summary>
/// Something scheduled for an application, e.g. "Technical interview on 10 Oct at 14:00".
/// </summary>
public class ApplicationEvent
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    public string Title { get; set; } = string.Empty;
    public EventType Type { get; set; }

    /// <summary>When it starts, in UTC. The browser converts to and from the user's local time.</summary>
    public DateTime StartsAt { get; set; }

    public string? Notes { get; set; }
}

public enum EventType
{
    OnlineTest,
    Interview,
    AssessmentCentre,
    Call,
    Other
}
