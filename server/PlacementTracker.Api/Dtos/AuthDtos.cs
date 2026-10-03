using System.ComponentModel.DataAnnotations;

namespace PlacementTracker.Api.Dtos;

/// <summary>Body for POST /api/auth/register and POST /api/auth/login.</summary>
public class AuthRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned after a successful register or login.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
