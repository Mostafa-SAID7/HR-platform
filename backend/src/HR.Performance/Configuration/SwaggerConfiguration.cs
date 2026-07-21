namespace HR.Performance.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

/// <summary>
/// Swagger/OpenAPI configuration for Performance service
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configure Swagger generation
    /// </summary>
    public static IServiceCollection AddPerformanceSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Performance Service",
                Version = "v1",
                Description = "Performance management service for HR Analytics Platform"
            });

            // Add JWT security definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            // Add JWT security requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger UI middleware
    /// </summary>
    public static WebApplication UsePerformanceSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Performance Service v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
