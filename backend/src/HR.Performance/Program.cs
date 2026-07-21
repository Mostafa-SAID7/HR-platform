using HR.Performance.Infrastructure.Persistence;
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
builder.Services.AddDbContext<PerformanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<PerformanceDbContext>();
builder.Services.AddOutboxPattern<PerformanceDbContext>();
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

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
    .AddMicroserviceHealthChecks("Performance Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Performance Service",
        Version = "v1",
        Description = "Performance management service for HR Analytics Platform"
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
    var db = scope.ServiceProvider.GetRequiredService<PerformanceDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Performance Service v1");
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
var apiGroup = app.MapGroup("/performance")
    .WithTags("Performance Reviews");

// Create performance review
apiGroup.MapPost("/reviews", CreateReviewEndpoint.Handle)
    .WithName("CreatePerformanceReview")
    .WithOpenApi()
    .RequireAuthorization();

// Get performance reviews (with filtering and pagination)
apiGroup.MapGet("/reviews", GetReviewsEndpoint.Handle)
    .WithName("GetPerformanceReviews")
    .WithOpenApi()
    .RequireAuthorization();

// Get performance review by ID
apiGroup.MapGet("/reviews/{id:guid}", GetReviewByIdEndpoint.Handle)
    .WithName("GetPerformanceReviewById")
    .WithOpenApi()
    .RequireAuthorization();

// Submit performance review
apiGroup.MapPost("/reviews/{id:guid}/submit", SubmitReviewEndpoint.Handle)
    .WithName("SubmitPerformanceReview")
    .WithOpenApi()
    .RequireAuthorization();

// Approve performance review
apiGroup.MapPost("/reviews/{id:guid}/approve", ApproveReviewEndpoint.Handle)
    .WithName("ApprovePerformanceReview")
    .WithOpenApi()
    .RequireAuthorization();

// Set ratings
apiGroup.MapPost("/reviews/{id:guid}/ratings", SetRatingsEndpoint.Handle)
    .WithName("SetPerformanceRatings")
    .WithOpenApi()
    .RequireAuthorization();

// Add feedback
apiGroup.MapPost("/reviews/{id:guid}/feedback", AddFeedbackEndpoint.Handle)
    .WithName("AddPerformanceFeedback")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Performance
{
    public partial class Program { }
}

// Endpoint handlers
public static class CreateReviewEndpoint
{
    public static async Task<IResult> Handle(
        CreatePerformanceReviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreatePerformanceReviewCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/performance/reviews/{result.Id}", ApiResponse<PerformanceReviewDetailDto>.Created(result));
    }
}

public static class GetReviewsEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] PerformanceFilterDto filter,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPerformanceReviewsQuery(filter, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<PerformanceReviewListDto>>.Ok(result));
    }
}

public static class GetReviewByIdEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPerformanceReviewByIdQuery(id, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}

public static class SubmitReviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SubmitPerformanceReviewCommand(id, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Performance review submitted successfully"));
    }
}

public static class ApproveReviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ApprovePerformanceReviewCommand(id, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Performance review approved successfully"));
    }
}

public static class SetRatingsEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        SetPerformanceRatingsRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SetPerformanceRatingsCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}

public static class AddFeedbackEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        AddFeedbackRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new AddPerformanceFeedbackCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}

// Import required namespaces for endpoints
using HR.Performance.Features.CreatePerformanceReview;
using HR.Performance.Features.GetPerformanceReviews;
using HR.Performance.Features.GetPerformanceReviewById;
using HR.Performance.Features.SubmitPerformanceReview;
using HR.Performance.Features.ApprovePerformanceReview;
using HR.Performance.Features.SetPerformanceRatings;
using HR.Performance.Features.AddPerformanceFeedback;
using HR.Common;
using Microsoft.AspNetCore.Mvc;
