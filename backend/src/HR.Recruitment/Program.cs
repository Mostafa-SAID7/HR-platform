using HR.Recruitment.Infrastructure.Persistence;
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
builder.Services.AddDbContext<RecruitmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<RecruitmentDbContext>();
builder.Services.AddOutboxPattern<RecruitmentDbContext>();
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add Kafka messaging
builder.Services.AddKafkaMessaging(builder.Configuration, Assembly.GetExecutingAssembly());

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
    .AddMicroserviceHealthChecks("Recruitment Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Recruitment Service",
        Version = "v1",
        Description = "Recruitment service for HR Analytics Platform"
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
    var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recruitment Service v1");
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
var apiGroup = app.MapGroup("/recruitment")
    .WithTags("Recruitment");

// Job Postings
apiGroup.MapPost("/job-postings", CreateJobPostingEndpoint.Handle)
    .WithName("CreateJobPosting")
    .WithOpenApi()
    .RequireAuthorization();

apiGroup.MapGet("/job-postings", GetJobPostingsEndpoint.Handle)
    .WithName("GetJobPostings")
    .WithOpenApi();

apiGroup.MapPost("/job-postings/{id:guid}/publish", PublishJobPostingEndpoint.Handle)
    .WithName("PublishJobPosting")
    .WithOpenApi()
    .RequireAuthorization();

// Job Applications
apiGroup.MapPost("/job-postings/{jobPostingId:guid}/apply", ApplyJobEndpoint.Handle)
    .WithName("ApplyJob")
    .WithOpenApi();

apiGroup.MapGet("/job-postings/{jobPostingId:guid}/applications", GetApplicationsEndpoint.Handle)
    .WithName("GetApplications")
    .WithOpenApi()
    .RequireAuthorization();

// Interviews
apiGroup.MapPost("/applications/{applicationId:guid}/schedule-interview", ScheduleInterviewEndpoint.Handle)
    .WithName("ScheduleInterview")
    .WithOpenApi()
    .RequireAuthorization();

// Offers
apiGroup.MapPost("/offer-letters", CreateOfferLetterEndpoint.Handle)
    .WithName("CreateOfferLetter")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Recruitment
{
    public partial class Program { }
}

// Endpoint handlers
public static class CreateJobPostingEndpoint
{
    public static async Task<IResult> Handle(
        CreateJobPostingRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateJobPostingCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/job-postings/{result.Id}", ApiResponse<JobPostingDto>.Created(result));
    }
}

public static class GetJobPostingsEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] JobPostingFilterDto filter,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetJobPostingsQuery(filter, Guid.Empty); // TenantId can be extracted from context if needed
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<JobPostingListDto>>.Ok(result));
    }
}

public static class PublishJobPostingEndpoint
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

        var command = new PublishJobPostingCommand(id, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<JobPostingDto>.Ok(result));
    }
}

public static class ApplyJobEndpoint
{
    public static async Task<IResult> Handle(
        Guid jobPostingId,
        ApplyJobRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ApplyJobCommand(jobPostingId, request, Guid.Empty);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/applications/{result.Id}", ApiResponse<JobApplicationDto>.Created(result));
    }
}

public static class GetApplicationsEndpoint
{
    public static async Task<IResult> Handle(
        Guid jobPostingId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetApplicationsQuery(jobPostingId, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<List<JobApplicationDetailDto>>.Ok(result));
    }
}

public static class ScheduleInterviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid applicationId,
        ScheduleInterviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ScheduleInterviewCommand(applicationId, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/interviews/{result.Id}", ApiResponse<InterviewScheduleDto>.Created(result));
    }
}

public static class CreateOfferLetterEndpoint
{
    public static async Task<IResult> Handle(
        CreateOfferLetterRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateOfferLetterCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/offer-letters/{result.Id}", ApiResponse<OfferLetterDto>.Created(result));
    }
}

// Import required namespaces for endpoints
using HR.Recruitment.Features.CreateJobPosting;
using HR.Recruitment.Features.GetJobPostings;
using HR.Recruitment.Features.PublishJobPosting;
using HR.Recruitment.Features.ApplyJob;
using HR.Recruitment.Features.ScheduleInterview;
using HR.Recruitment.Features.CreateOfferLetter;
using Microsoft.AspNetCore.Mvc;
