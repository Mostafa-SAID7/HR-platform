using HR.Employee.Infrastructure.Persistence;
using HR.Employee.Configuration;
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

builder.Services.AddEmployeeService(connectionString);
builder.Services.AddEmployeeAuthentication();
builder.Services.AddEmployeeAuthorization();
builder.Services.AddEmployeeCors();
builder.Services.AddEmployeeHealthChecks(builder.Configuration);
builder.Services.AddEmployeeSwagger();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseEmployeeSwagger();
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
app.MapEmployeeRoutes();

app.Run();

namespace HR.Employee
{
    public partial class Program { }
}
