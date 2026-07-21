using HR.Notification.Infrastructure.Persistence;
using HR.Notification.Application.Services;
using HR.Notification.Configuration;
using Serilog;
using System.Reflection;
using HR.Common;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services - using split Configuration extension methods
builder.Services.AddNotificationService(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");

// Register notification channel services
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<PushNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();
builder.Services.AddScoped<INotificationService, CompositeNotificationService>();

// Add database connection for Dapper queries
builder.Services.AddScoped<IQueryRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var connection = new Npgsql.NpgsqlConnection(connectionString);
    return new DapperQueryRepository(connection);
});

// Add Kafka messaging
builder.Services.AddKafkaMessaging(builder.Configuration, Assembly.GetExecutingAssembly());

// Add authentication & authorization
builder.Services.AddNotificationAuthentication();
builder.Services.AddNotificationAuthorization();

// Add CORS
builder.Services.AddNotificationCors();

// Add health checks
builder.Services.AddNotificationHealthChecks(builder.Configuration);

// Add Swagger
builder.Services.AddNotificationSwagger();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseNotificationSwagger();

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
app.MapNotificationRoutes();

app.Run();

namespace HR.Notification
{
    public partial class Program { }
}
