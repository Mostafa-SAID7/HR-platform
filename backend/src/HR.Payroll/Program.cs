using HR.Payroll.Infrastructure.Persistence;
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
builder.Services.AddDbContext<PayrollDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<PayrollDbContext>();
builder.Services.AddOutboxPattern<PayrollDbContext>();
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
            .WithOrigins("http://localhost:5000", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddMicroserviceHealthChecks("Payroll Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Payroll Service",
        Version = "v1",
        Description = "Payroll calculation, processing, and reporting service"
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
    var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payroll Service v1");
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
var apiGroup = app.MapGroup("/payroll")
    .WithTags("Payroll");

// Calculate payroll
apiGroup.MapPost("/calculate", CalculatePayrollEndpoint.Handle)
    .WithName("CalculatePayroll")
    .RequireAuthorization();

// Add deduction
apiGroup.MapPost("/deduction", AddDeductionEndpoint.Handle)
    .WithName("AddDeduction")
    .RequireAuthorization();

// Get payslip
apiGroup.MapGet("/payslip/{payrollRecordId:guid}", GetPayslipEndpoint.Handle)
    .WithName("GetPayslip")
    .RequireAuthorization();

// Get payroll report
apiGroup.MapGet("/report/{year:int}/{month:int}", GetPayrollReportEndpoint.Handle)
    .WithName("GetPayrollReport")
    .RequireAuthorization();

// Approve payroll
apiGroup.MapPost("/approve/{payrollRecordId:guid}", ApprovePayrollEndpoint.Handle)
    .WithName("ApprovePayroll")
    .RequireAuthorization();

// Process payment
apiGroup.MapPost("/process-payment/{payrollRecordId:guid}", ProcessPaymentEndpoint.Handle)
    .WithName("ProcessPayment")
    .RequireAuthorization();

app.Run();

namespace HR.Payroll
{
    public partial class Program { }
}

// Endpoint handlers
public static class CalculatePayrollEndpoint
{
    public static async Task<IResult> Handle(
        CalculatePayrollRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CalculatePayrollCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PayrollRecordDto>.Ok(result));
    }
}

public static class AddDeductionEndpoint
{
    public static async Task<IResult> Handle(
        AddDeductionRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new AddDeductionCommand(request, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Created($"/payroll/deduction/{request.PayrollRecordId}", ApiResponse.Ok("Deduction added successfully"));
    }
}

public static class GetPayslipEndpoint
{
    public static async Task<IResult> Handle(
        Guid payrollRecordId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPayslipQuery(payrollRecordId, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PayslipDto>.Ok(result));
    }
}

public static class GetPayrollReportEndpoint
{
    public static async Task<IResult> Handle(
        int year,
        int month,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPayrollReportQuery(year, month, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PayrollReportDto>.Ok(result));
    }
}

public static class ApprovePayrollEndpoint
{
    public static async Task<IResult> Handle(
        Guid payrollRecordId,
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

        var command = new ApprovePayrollCommand(payrollRecordId, approvedBy, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Payroll approved successfully"));
    }
}

public static class ProcessPaymentEndpoint
{
    public static async Task<IResult> Handle(
        Guid payrollRecordId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ProcessPaymentCommand(payrollRecordId, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Payment processed successfully"));
    }
}

// Imports
using HR.Payroll.Features.CalculatePayroll;
using HR.Payroll.Features.AddDeduction;
using HR.Payroll.Features.GetPayslip;
using HR.Payroll.Features.GetPayrollReport;
using HR.Payroll.Features.ApprovePayroll;
using HR.Payroll.Features.ProcessPayment;
using HR.Common;
using Microsoft.AspNetCore.Mvc;
