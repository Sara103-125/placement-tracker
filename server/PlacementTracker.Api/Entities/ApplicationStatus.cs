namespace PlacementTracker.Api.Entities;

/// <summary>
/// Pipeline stages for a placement/internship application.
/// Stored in the database as strings so they stay readable in SQL Server.
/// </summary>
public enum ApplicationStatus
{
    Wishlist,
    Applied,
    OnlineTest,
    Interview,
    Offer,
    Rejected
}
