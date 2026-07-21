namespace HR.Recruitment.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

/// <summary>
/// Swagger/OpenAPI configuration for Recruitment service
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configure Swagger generation
    /// </summary>
    public static IServiceCollection AddRecruitmentSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Recruitment Service",
                Version = "v1",
                Description = "Recruitment service for HR Analytics Platform"
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
    public static WebApplication UseRecruitmentSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recruitment Service v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
