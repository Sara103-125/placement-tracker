using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Dtos;

/// <summary>
/// Everything the dashboard page needs, returned in one request.
/// </summary>
public class DashboardSummary
{
    public int Total { get; set; }

    /// <summary>Count per status, e.g. { "Applied": 4, "Interview": 1, ... }. Every status is included, even if 0.</summary>
    public Dictionary<ApplicationStatus, int> StatusCounts { get; set; } = new();

    /// <summary>How many applications reached each stage (or a later one) at some point. See DashboardController.</summary>
    public List<FunnelStep> Funnel { get; set; } = new();

    /// <summary>Applications with a deadline from today up to 7 days ahead, soonest first.</summary>
    public List<JobApplication> UpcomingDeadlines { get; set; } = new();
}

public class FunnelStep
{
    public ApplicationStatus Status { get; set; }
    public int Count { get; set; }
}
