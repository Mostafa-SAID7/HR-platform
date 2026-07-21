using HR.Analytics.Infrastructure.Persistence;
using HR.Analytics.Application.Services;
using Elastic.Clients.Elasticsearch;
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
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.AddUnitOfWork<AnalyticsDbContext>();
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add Kafka messaging
builder.Services.AddKafkaMessaging(builder.Configuration, Assembly.GetExecutingAssembly());

// Add Elasticsearch
var elasticsearchUri = builder.Configuration["ConnectionStrings:Elasticsearch"] ?? "http://localhost:9200";
var client = new ElasticsearchClient(new Uri(elasticsearchUri));
builder.Services.AddSingleton(client);
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

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
    .AddMicroserviceHealthChecks("Analytics Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Analytics Service",
        Version = "v1",
        Description = "Analytics and reporting service with Elasticsearch integration"
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
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Analytics Service v1");
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
var apiGroup = app.MapGroup("/analytics")
    .WithTags("Analytics");

// Search
apiGroup.MapGet("/search", SearchEndpoint.Handle)
    .WithName("Search")
    .RequireAuthorization();

// Dashboard
apiGroup.MapGet("/dashboard", GetDashboardEndpoint.Handle)
    .WithName("GetDashboard")
    .RequireAuthorization();

app.Run();

namespace HR.Analytics
{
    public partial class Program { }
}

// Endpoint handlers
public static class SearchEndpoint
{
    public static async Task<IResult> Handle(
        string searchTerm,
        int pageSize = 20,
        int pageNumber = 1,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new SearchEmployeesQuery(searchTerm, pageSize, pageNumber, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<SearchResultDto<EmployeeAnalyticsDto>>.Ok(result));
    }
}

public static class GetDashboardEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetDashboardMetricsQuery(tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<DashboardMetricsDto>.Ok(result));
    }
}

// Imports
using HR.Analytics.Features.Search;
using HR.Analytics.Features.Dashboard;
using HR.Common;
using Microsoft.AspNetCore.Mvc;
