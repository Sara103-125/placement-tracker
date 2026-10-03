using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlacementTracker.Api.Auth;
using PlacementTracker.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Send and receive enums as text ("Interview") instead of numbers (3) in JSON.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddEndpointsApiExplorer();

// Swagger with an "Authorize" button, so you can paste a token and test the protected endpoints.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Log in with POST /api/auth/login, then paste the token here.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Login with JWTs ---
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters long.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<TokenService>();

// Every request with "Authorization: Bearer <token>" has its token checked here:
// right signature, right issuer/audience, not expired. [Authorize] endpoints reject anything else with 401.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply any pending migrations on startup, so the database (LocalDB here, Azure SQL when live)
// always matches the code without running "dotnet ef database update" by hand.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// When live, the built Angular website is copied into wwwroot, so this one app serves both
// the website and the API from a single address. (Locally, `ng serve` serves the website instead.)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication(); // who is this? (reads the token)
app.UseAuthorization();  // are they allowed? (checks [Authorize])
app.MapControllers();

// Any other address that isn't an API route or a file (e.g. /dashboard) is an Angular page,
// so send index.html and let Angular's router show the right page.
app.MapFallbackToFile("index.html");

app.Run();
