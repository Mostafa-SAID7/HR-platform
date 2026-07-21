using HR.ApiGateway.Configuration;
using HR.ApiGateway.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
var gatewayOptions = builder.Configuration.GetSection(GatewayOptions.SectionName).Get<GatewayOptions>() 
    ?? new GatewayOptions();

builder.Services.Configure<GatewayOptions>(builder.Configuration.GetSection(GatewayOptions.SectionName));

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimit"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("gateway", new ServiceHealthCheck("API Gateway"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HR Analytics API Gateway",
        Version = "v1",
        Description = "API Gateway for HR Analytics Platform - routes requests to microservices"
    });

    // Add Bearer token to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Bearer token"
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

// Configure middleware pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Analytics API Gateway v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("AllowAll");

// Add gateway middleware
app.UseCorrelationId();
app.UseRequestResponseLogging();
app.UseGatewayAuthentication();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// YARP reverse proxy
app.MapReverseProxy();

// Custom endpoints
app.MapGet("/gateway/info", () => new
{
    service = "HR Analytics API Gateway",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow,
    routes = new[]
    {
        new { path = "/identity", service = "Identity Service", port = 5001 },
        new { path = "/employee", service = "Employee Service", port = 5002 },
        new { path = "/performance", service = "Performance Service", port = 5003 },
        new { path = "/recruitment", service = "Recruitment Service", port = 5004 },
        new { path = "/attendance", service = "Attendance Service", port = 5005 },
        new { path = "/payroll", service = "Payroll Service", port = 5006 },
        new { path = "/analytics", service = "Analytics Service", port = 5007 },
        new { path = "/notification", service = "Notification Service", port = 5008 },
        new { path = "/audit", service = "Audit Service", port = 5009 }
    }
}).WithName("GetGatewayInfo");

app.Run();

namespace HR.ApiGateway
{
    // For test purposes
    public partial class Program { }
}
