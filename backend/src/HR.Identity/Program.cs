using HR.Identity.Infrastructure.Persistence;
using HR.Identity.Application.Services;
using HR.Identity.Configuration;
using HR.Common;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services - using split Configuration extension methods
builder.Services.AddIdentityService(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");

// Add JWT token and password services
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

// Add authentication & authorization
builder.Services.AddIdentityAuthentication();
builder.Services.AddIdentityAuthorization();

// Add CORS
builder.Services.AddIdentityCors();

// Add health checks
builder.Services.AddIdentityHealthChecks(builder.Configuration);

// Add Swagger
builder.Services.AddIdentitySwagger();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseIdentitySwagger();

app.UseRouting();
app.UseCors("AllowGateway");
app.UseCorrelationId();
app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Map API routes using split configuration
app.MapIdentityRoutes();

app.Run();

namespace HR.Identity
{
    public partial class Program { }
}
