namespace HR.Payroll.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

/// <summary>
/// Health checks configuration for Payroll service
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Configure health checks for database and dependencies
    /// </summary>
    public static IServiceCollection AddPayrollHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMicroserviceHealthChecks(
                "Payroll Service",
                postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));

        return services;
    }
}
