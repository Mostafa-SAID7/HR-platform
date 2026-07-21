namespace HR.Analytics.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddAnalyticsHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddMicroserviceHealthChecks("Analytics Service", postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
