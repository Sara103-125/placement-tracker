using System.Text.Json.Serialization;

namespace PlacementTracker.Api.Entities;

/// <summary>
/// One placement/internship application.
/// Named JobApplication so it is not confused with the ASP.NET "application".
/// </summary>
public class JobApplication
{
    public int Id { get; set; }

    /// <summary>
    /// The account that owns this application. Nullable only because applications created
    /// before accounts existed have no owner yet; the first account to register adopts them.
    /// </summary>
    [JsonIgnore]
    public int? UserId { get; set; }

    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? JobLink { get; set; }

    /// <summary>Null while the item is still on the wishlist.</summary>
    public DateOnly? DateApplied { get; set; }

    public DateOnly? Deadline { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Wishlist;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Every status this application has had. Not sent in normal JSON responses;
    /// it has its own endpoint: GET /api/applications/{id}/history.
    /// </summary>
    [JsonIgnore]
    public List<StatusChange> StatusHistory { get; set; } = new();

    /// <summary>Interviews, tests and calls for this application. Endpoint: /api/applications/{id}/events.</summary>
    [JsonIgnore]
    public List<ApplicationEvent> Events { get; set; } = new();
}
