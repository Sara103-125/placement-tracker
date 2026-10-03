namespace PlacementTracker.Api.Entities;

/// <summary>
/// A person with an account. Passwords are never stored, only a salted hash of them.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Always stored lower-case, so "Sam@Mail.com" and "sam@mail.com" are the same account.</summary>
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
