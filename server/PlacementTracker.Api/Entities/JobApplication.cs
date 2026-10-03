namespace PlacementTracker.Api.Entities;

/// <summary>
/// One placement/internship application.
/// Named JobApplication so it is not confused with the ASP.NET "application".
/// </summary>
public class JobApplication
{
    public int Id { get; set; }

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
}
