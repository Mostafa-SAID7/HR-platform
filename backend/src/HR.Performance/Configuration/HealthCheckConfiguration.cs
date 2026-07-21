namespace HR.Performance.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

/// <summary>
/// Health checks configuration for Performance service
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Configure health checks for database and dependencies
    /// </summary>
    public static IServiceCollection AddPerformanceHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMicroserviceHealthChecks(
                "Performance Service",
                postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));

        return services;
    }
}
