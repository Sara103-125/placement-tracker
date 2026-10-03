namespace PlacementTracker.Api.Auth;

/// <summary>
/// The "Jwt" section of appsettings. The Key signs every token, so it must be secret:
/// in development it lives in appsettings.Development.json; in production it comes from
/// an environment variable (Jwt__Key) and is never committed.
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryDays { get; set; } = 7;
}
