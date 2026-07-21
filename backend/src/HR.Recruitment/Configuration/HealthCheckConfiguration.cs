namespace HR.Recruitment.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

/// <summary>
/// Health checks configuration for Recruitment service
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Configure health checks for database and dependencies
    /// </summary>
    public static IServiceCollection AddRecruitmentHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMicroserviceHealthChecks(
                "Recruitment Service",
                postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));

        return services;
    }
}
