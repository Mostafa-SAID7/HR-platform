using HR.Analytics.Infrastructure.Persistence;
using HR.Analytics.Configuration;
using Serilog;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services using configuration extension methods organized by concern
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddAnalyticsService(connectionString, builder.Configuration);
builder.Services.AddAnalyticsAuthentication();
builder.Services.AddAnalyticsAuthorization();
builder.Services.AddAnalyticsCors();
builder.Services.AddAnalyticsHealthChecks(builder.Configuration);
builder.Services.AddAnalyticsSwagger();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseAnalyticsSwagger();
app.UseRouting();
app.UseCors("AllowGateway");
app.UseCorrelationId();
app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Map API routes
app.MapAnalyticsRoutes();

app.Run();

namespace HR.Analytics
{
    public partial class Program { }
}
