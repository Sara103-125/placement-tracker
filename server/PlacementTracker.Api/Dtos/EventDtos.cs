using System.ComponentModel.DataAnnotations;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Dtos;

/// <summary>Body for POST /api/applications/{id}/events.</summary>
public class EventRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public EventType Type { get; set; } = EventType.Interview;

    /// <summary>The browser sends this in UTC, e.g. "2026-10-10T13:00:00Z".</summary>
    [Required]
    public DateTime? StartsAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>An event plus the company and role it belongs to, for the dashboard.</summary>
public class UpcomingEvent
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public DateTime StartsAt { get; set; }
}
