using HR.Identity.Infrastructure.Persistence;
using HR.Identity.Application.Services;
using HR.Common;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCommonServices(
    Assembly.GetExecutingAssembly(),
    typeof(Program).Assembly);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddUnitOfWork<IdentityDbContext>();
builder.Services.AddOutboxPattern<IdentityDbContext>();

// Add Redis
builder.Services.AddRedisCache(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Add health checks
builder.Services.AddHealthChecks()
    .AddMicroserviceHealthChecks("Identity Service",
        postgresConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"));

// Add authentication
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var jwtOptions = new JwtOptions();
        builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Bearer", policy =>
    {
        policy.AuthenticationSchemes.Add("Bearer");
        policy.RequireAuthenticatedUser();
    });

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

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity Service",
        Version = "v1",
        Description = "Authentication and authorization service for HR Analytics Platform"
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
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1");
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
var apiGroup = app.MapGroup("/identity")
    .WithTags("Identity");

// Login
apiGroup.MapPost("/login", LoginEndpoint.Handle)
    .WithName("Login")
    .WithOpenApi()
    .AllowAnonymous();

// Profile
apiGroup.MapGet("/profile", ProfileEndpoint.Handle)
    .WithName("GetProfile")
    .WithOpenApi()
    .RequireAuthorization("Bearer");

app.Run();

namespace HR.Identity
{
    public partial class Program { }
}

// Endpoint definitions
public static class LoginEndpoint
{
    public static async Task<IResult> Handle(
        LoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(result));
    }
}

public static class ProfileEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetUserProfileQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<UserProfileDto>.Ok(result));
    }
}

// Import required namespaces for endpoints
using HR.Identity.Application.Dtos;
using HR.Identity.Features.Login;
using HR.Identity.Features.Profile;
using System.Security.Claims;
