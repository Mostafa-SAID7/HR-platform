using Serilog;
using System.Reflection;
using HR.Common;
using HR.Audit.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddMediatR(config => 
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMapster();

builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add Kafka messaging for consuming events
builder.Services.AddKafkaMessaging(builder.Configuration, Assembly.GetExecutingAssembly());

// Register audit event consumer
builder.Services.AddScoped<AuditEventConsumer>();

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
    .AddMicroserviceHealthChecks("Audit Service",
        redisConnectionString: builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Audit Service",
        Version = "v1",
        Description = "Audit and compliance service for HR Analytics Platform (Event-sourced from Kafka)"
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

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit Service v1");
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
var apiGroup = app.MapGroup("/audit")
    .WithTags("Audit");

// Get audit trail for entity
apiGroup.MapGet("/trail/{entityType}/{entityId:guid}", GetAuditTrailEndpoint.Handle)
    .WithName("GetAuditTrail")
    .WithOpenApi()
    .RequireAuthorization();

// Create audit report
apiGroup.MapPost("/reports", CreateAuditReportEndpoint.Handle)
    .WithName("CreateAuditReport")
    .WithOpenApi()
    .RequireAuthorization();

// Get audit trail summary
apiGroup.MapGet("/summary/{entityType}/{entityId:guid}", GetAuditTrailSummaryEndpoint.Handle)
    .WithName("GetAuditTrailSummary")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Audit
{
    public partial class Program { }
}

// Endpoint handlers
public static class GetAuditTrailEndpoint
{
    public static async Task<IResult> Handle(
        string entityType,
        Guid entityId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetAuditTrailQuery(entityId, entityType, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<AuditTrailDetailDto>.Ok(result));
    }
}

public static class CreateAuditReportEndpoint
{
    public static async Task<IResult> Handle(
        CreateAuditReportRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.BadRequest("Invalid user");

        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateAuditReportCommand(request, userId, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/audit/reports/{result.Id}", ApiResponse<AuditReportDto>.Created(result));
    }
}

public static class GetAuditTrailSummaryEndpoint
{
    public static async Task<IResult> Handle(
        string entityType,
        Guid entityId,
        HttpContext context,
        IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        try
        {
            var trailKey = $"audit:trail:{entityType}:{entityId}";
            var trailJson = await cache.GetStringAsync(trailKey, cancellationToken);

            if (string.IsNullOrEmpty(trailJson))
            {
                return Results.NotFound($"No audit trail found for {entityType}:{entityId}");
            }

            var trail = System.Text.Json.JsonSerializer.Deserialize<AuditTrail>(trailJson);
            if (trail == null)
                return Results.BadRequest("Failed to deserialize audit trail");

            var summary = trail.GetSummary();
            var summaryDto = new AuditTrailSummaryDto(
                summary.EntityId,
                summary.EntityType,
                summary.FirstChangeAt,
                summary.LastChangeAt,
                summary.ChangeCount,
                summary.AffectedUsers,
                summary.CriticalEventCount,
                summary.WarningEventCount,
                summary.InfoEventCount);

            return Results.Ok(ApiResponse<AuditTrailSummaryDto>.Ok(summaryDto));
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error retrieving audit trail summary: {ex.Message}");
        }
    }
}

// Import required namespaces for endpoints
using HR.Audit.Features.GetAuditTrail;
using HR.Audit.Features.CreateAuditReport;
using Microsoft.AspNetCore.Mvc;
