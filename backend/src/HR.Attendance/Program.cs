using HR.Attendance.Infrastructure.Persistence;
using HR.Attendance.Hubs;
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
builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<AttendanceDbContext>();
builder.Services.AddOutboxPattern<AttendanceDbContext>();
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add SignalR
builder.Services.AddSignalR();

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
            .WithOrigins("http://localhost:5000", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for SignalR
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddMicroserviceHealthChecks("Attendance Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Attendance Service",
        Version = "v1",
        Description = "Real-time attendance and leave management service"
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
    var db = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance Service v1");
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

// SignalR Hub
app.MapHub<AttendanceHub>("/hubs/attendance");

// API endpoints
var apiGroup = app.MapGroup("/attendance")
    .WithTags("Attendance");

// Check-in
apiGroup.MapPost("/check-in", CheckInEndpoint.Handle)
    .WithName("CheckIn")
    .WithOpenApi()
    .RequireAuthorization();

// Check-out
apiGroup.MapPost("/check-out", CheckOutEndpoint.Handle)
    .WithName("CheckOut")
    .WithOpenApi()
    .RequireAuthorization();

// Get today's attendance
apiGroup.MapGet("/today/{employeeId:guid}", GetTodayAttendanceEndpoint.Handle)
    .WithName("GetTodayAttendance")
    .WithOpenApi()
    .RequireAuthorization();

// Request leave
apiGroup.MapPost("/leave-request", RequestLeaveEndpoint.Handle)
    .WithName("RequestLeave")
    .WithOpenApi()
    .RequireAuthorization();

// Get leave requests
apiGroup.MapGet("/leave-requests", GetLeaveRequestsEndpoint.Handle)
    .WithName("GetLeaveRequests")
    .WithOpenApi()
    .RequireAuthorization();

// Approve leave
apiGroup.MapPost("/leave-requests/{id:guid}/approve", ApproveLeaveEndpoint.Handle)
    .WithName("ApproveLeave")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Attendance
{
    public partial class Program { }
}

// Endpoint handlers
public static class CheckInEndpoint
{
    public static async Task<IResult> Handle(
        CheckInRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CheckInCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<AttendanceRecordDto>.Ok(result));
    }
}

public static class CheckOutEndpoint
{
    public static async Task<IResult> Handle(
        CheckOutRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CheckOutCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<AttendanceRecordDto>.Ok(result));
    }
}

public static class GetTodayAttendanceEndpoint
{
    public static async Task<IResult> Handle(
        Guid employeeId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetTodayAttendanceQuery(employeeId, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<AttendanceRecordDto>.Ok(result));
    }
}

public static class RequestLeaveEndpoint
{
    public static async Task<IResult> Handle(
        LeaveRequestDto request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new RequestLeaveCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/attendance/leave-requests/{result.Id}", ApiResponse<LeaveRequestDto>.Created(result));
    }
}

public static class GetLeaveRequestsEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetLeaveRequestsQuery(tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<List<LeaveRequestDto>>.Ok(result));
    }
}

public static class ApproveLeaveEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");
        
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var approvedBy))
            return Results.BadRequest("Invalid user");

        var command = new ApproveLeaveCommand(id, approvedBy, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Leave request approved successfully"));
    }
}

// Import required namespaces
using HR.Attendance.Features.CheckIn;
using HR.Attendance.Features.CheckOut;
using HR.Attendance.Features.GetTodayAttendance;
using HR.Attendance.Features.RequestLeave;
using HR.Attendance.Features.GetLeaveRequests;
using HR.Attendance.Features.ApproveLeave;
using HR.Common;
using Microsoft.AspNetCore.Mvc;
