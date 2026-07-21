using HR.Notification.Infrastructure.Persistence;
using HR.Notification.Application.Services;
using Serilog;
using System.Reflection;
using HR.Common;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<NotificationDbContext>();
builder.Services.AddOutboxPattern<NotificationDbContext>();
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add Kafka messaging
builder.Services.AddKafkaMessaging(builder.Configuration, Assembly.GetExecutingAssembly());

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

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddMicroserviceHealthChecks("Notification Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Service",
        Version = "v1",
        Description = "Notification service for HR Analytics Platform"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("AllowGateway");
app.UseCorrelationId();
app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// API endpoints
var apiGroup = app.MapGroup("/notification")
    .WithTags("Notifications");

// Send notification
apiGroup.MapPost("/send", SendNotificationEndpoint.Handle)
    .WithName("SendNotification")
    .WithOpenApi()
    .RequireAuthorization();

// Get notifications
apiGroup.MapGet("/", GetNotificationsEndpoint.Handle)
    .WithName("GetNotifications")
    .WithOpenApi()
    .RequireAuthorization();

// Get user preferences
apiGroup.MapGet("/preferences", GetPreferencesEndpoint.Handle)
    .WithName("GetPreferences")
    .WithOpenApi()
    .RequireAuthorization();

// Update preferences
apiGroup.MapPut("/preferences", UpdatePreferencesEndpoint.Handle)
    .WithName("UpdatePreferences")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Notification
{
    public partial class Program { }
}

// Endpoint handlers
public static class SendNotificationEndpoint
{
    public static async Task<IResult> Handle(
        SendNotificationRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SendNotificationCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/notification/{result.Id}", ApiResponse<NotificationDto>.Created(result));
    }
}

public static class GetNotificationsEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] NotificationFilterDto filter,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.BadRequest("Invalid user");

        var queryFilter = new NotificationFilterDto(
            RecipientId: userId,
            Status: filter.Status,
            Page: filter.Page,
            PageSize: filter.PageSize);

        var query = new GetNotificationsQuery(queryFilter, Guid.Empty);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationListDto>>.Ok(result));
    }
}

public static class GetPreferencesEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext context,
        NotificationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.BadRequest("Invalid user");

        var preference = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (preference == null)
        {
            return Results.Ok(ApiResponse<NotificationPreferenceDto>.Ok(
                new NotificationPreferenceDto(userId, true, true, true, true, true)));
        }

        return Results.Ok(ApiResponse<NotificationPreferenceDto>.Ok(preference.Adapt<NotificationPreferenceDto>()));
    }
}

public static class UpdatePreferencesEndpoint
{
    public static async Task<IResult> Handle(
        UpdateNotificationPreferenceRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.BadRequest("Invalid user");

        var command = new UpdateNotificationPreferenceCommand(userId, request, Guid.Empty);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<NotificationPreferenceDto>.Ok(result));
    }
}

// Import required namespaces for endpoints
using HR.Notification.Features.SendNotification;
using HR.Notification.Features.GetNotifications;
using HR.Notification.Features.UpdatePreference;
using Microsoft.AspNetCore.Mvc;
