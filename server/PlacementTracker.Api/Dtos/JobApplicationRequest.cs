using System.ComponentModel.DataAnnotations;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Dtos;

/// <summary>
/// The data the client sends when creating or updating an application.
/// Kept separate from the entity so the client cannot set Id, CreatedAt or UpdatedAt.
/// </summary>
public class JobApplicationRequest
{
    [Required, MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [Url, MaxLength(500)]
    public string? JobLink { get; set; }

    public DateOnly? DateApplied { get; set; }
    public DateOnly? Deadline { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Wishlist;

    [MaxLength(4000)]
    public string? Notes { get; set; }
}
