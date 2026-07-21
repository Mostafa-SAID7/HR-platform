namespace HR.Performance.Configuration;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// CORS configuration for Performance service
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configure CORS policies
    /// </summary>
    public static IServiceCollection AddPerformanceCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowGateway", policyBuilder =>
            {
                policyBuilder
                    .WithOrigins("http://localhost:5000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
