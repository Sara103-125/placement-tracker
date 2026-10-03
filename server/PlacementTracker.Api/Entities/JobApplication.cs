namespace PlacementTracker.Api.Entities;

/// <summary>
/// One placement/internship application belonging to a single user.
/// Named JobApplication so it is not confused with the ASP.NET "application".
/// </summary>
public class JobApplication
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public string Company { get; set; } = default!;
    public string Role { get; set; } = default!;
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
