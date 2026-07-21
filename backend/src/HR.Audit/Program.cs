using HR.Audit.Configuration;
using Serilog;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services using configuration extension methods organized by concern
builder.Services.AddAuditService(builder.Configuration);
builder.Services.AddAuditAuthentication();
builder.Services.AddAuditAuthorization();
builder.Services.AddAuditCors();
builder.Services.AddAuditHealthChecks(builder.Configuration);
builder.Services.AddAuditSwagger();

var app = builder.Build();

// Configure middleware
app.UseAuditSwagger();
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
app.MapAuditRoutes();

app.Run();

namespace HR.Audit
{
    public partial class Program { }
}
