using HR.Payroll.Infrastructure.Persistence;
using HR.Payroll.Configuration;
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

builder.Services.AddPayrollService(connectionString);
builder.Services.AddPayrollAuthentication();
builder.Services.AddPayrollAuthorization();
builder.Services.AddPayrollCors();
builder.Services.AddPayrollHealthChecks(builder.Configuration);
builder.Services.AddPayrollSwagger();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UsePayrollSwagger();
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
app.MapPayrollRoutes();

app.Run();

namespace HR.Payroll
{
    public partial class Program { }
}
