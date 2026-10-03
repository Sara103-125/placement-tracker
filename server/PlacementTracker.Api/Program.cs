using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PlacementTracker.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Send and receive enums as text ("Interview") instead of numbers (3) in JSON.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Apply any pending migrations on startup so the database is always up to date locally.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();
