using HR.Employee.Infrastructure.Persistence;
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
builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<EmployeeDbContext>();
builder.Services.AddOutboxPattern<EmployeeDbContext>();
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
    .AddMicroserviceHealthChecks("Employee Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Employee Service",
        Version = "v1",
        Description = "Employee management service for HR Analytics Platform"
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
    var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Service v1");
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
var apiGroup = app.MapGroup("/employee")
    .WithTags("Employees");

// Create employee
apiGroup.MapPost("/", CreateEmployeeEndpoint.Handle)
    .WithName("CreateEmployee")
    .WithOpenApi()
    .RequireAuthorization();

// Get employees (with pagination and filtering)
apiGroup.MapGet("/", GetEmployeesEndpoint.Handle)
    .WithName("GetEmployees")
    .WithOpenApi()
    .RequireAuthorization();

// Get employee by ID
apiGroup.MapGet("/{id:guid}", GetEmployeeByIdEndpoint.Handle)
    .WithName("GetEmployeeById")
    .WithOpenApi()
    .RequireAuthorization();

// Update employee
apiGroup.MapPut("/{id:guid}", UpdateEmployeeEndpoint.Handle)
    .WithName("UpdateEmployee")
    .WithOpenApi()
    .RequireAuthorization();

// Terminate employee
apiGroup.MapPost("/{id:guid}/terminate", TerminateEmployeeEndpoint.Handle)
    .WithName("TerminateEmployee")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

namespace HR.Employee
{
    public partial class Program { }
}

// Endpoint handlers
public static class CreateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        CreateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateEmployeeCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/employee/{result.Id}", ApiResponse<EmployeeDetailDto>.Created(result));
    }
}

public static class GetEmployeesEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] EmployeeFilterDto filter,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetEmployeesQuery(filter, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<EmployeeListDto>>.Ok(result));
    }
}

public static class GetEmployeeByIdEndpoint
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

        var query = new GetEmployeeByIdQuery(id, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<EmployeeDetailDto>.Ok(result));
    }
}

public static class UpdateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        UpdateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new UpdateEmployeeCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<EmployeeDetailDto>.Ok(result));
    }
}

public static class TerminateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        TerminateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new TerminateEmployeeCommand(id, request.TerminationDate, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Employee terminated successfully"));
    }
}

// Import required namespaces for endpoints
using HR.Employee.Features.CreateEmployee;
using HR.Employee.Features.GetEmployees;
using HR.Employee.Features.GetEmployeeById;
using HR.Employee.Features.UpdateEmployee;
using HR.Employee.Features.TerminateEmployee;
using HR.Common;
using Microsoft.AspNetCore.Mvc;
