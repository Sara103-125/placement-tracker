using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Auth;
using PlacementTracker.Api.Data;
using PlacementTracker.Api.Dtos;
using PlacementTracker.Api.Entities;

namespace PlacementTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    // ASP.NET's built-in hasher: salts the password and runs PBKDF2 many times, so a leaked
    // database doesn't reveal anyone's password.
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    // POST api/auth/register   body: { "email": "...", "password": "..." }
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(AuthRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var isFirstUser = !await _db.Users.AnyAsync();

        var user = new User { Email = email, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Applications created before accounts existed have no owner: give them to the first account.
        if (isFirstUser)
        {
            var unowned = await _db.JobApplications.Where(a => a.UserId == null).ToListAsync();
            unowned.ForEach(a => a.UserId = user.Id);
            await _db.SaveChangesAsync();
        }

        return new AuthResponse { Token = _tokens.CreateToken(user), Email = user.Email };
    }

    // POST api/auth/login   body: { "email": "...", "password": "..." }
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message whether the email or the password is wrong, so attackers can't
        // find out which emails have accounts.
        if (user == null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Incorrect email or password." });
        }

        return new AuthResponse { Token = _tokens.CreateToken(user), Email = user.Email };
    }

    // GET api/auth/me   (needs a token) - lets the website check a saved token is still valid.
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user == null)
        {
            return Unauthorized();
        }

        return new { email = user.Email };
    }
}
