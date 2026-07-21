using HR.Recruitment.Infrastructure.Persistence;
using HR.Recruitment.Configuration;
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

builder.Services.AddRecruitmentService(connectionString);
builder.Services.AddRecruitmentAuthentication();
builder.Services.AddRecruitmentAuthorization();
builder.Services.AddRecruitmentCors();
builder.Services.AddRecruitmentHealthChecks(builder.Configuration);
builder.Services.AddRecruitmentSwagger();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseRecruitmentSwagger();
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
app.MapRecruitmentRoutes();

app.Run();

namespace HR.Recruitment
{
    public partial class Program { }
}
