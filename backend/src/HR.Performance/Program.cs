using HR.Performance.Infrastructure.Persistence;
using HR.Performance.Configuration;
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

builder.Services.AddPerformanceService(connectionString);
builder.Services.AddPerformanceAuthentication();
builder.Services.AddPerformanceAuthorization();
builder.Services.AddPerformanceCors();
builder.Services.AddPerformanceHealthChecks(builder.Configuration);
builder.Services.AddPerformanceSwagger();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PerformanceDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UsePerformanceSwagger();
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
app.MapPerformanceRoutes();

app.Run();

namespace HR.Performance
{
    public partial class Program { }
}
