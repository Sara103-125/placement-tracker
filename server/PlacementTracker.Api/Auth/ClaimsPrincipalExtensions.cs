using System.Security.Claims;

namespace PlacementTracker.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The logged-in user's id, read from their token. Use inside [Authorize] controllers.</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
