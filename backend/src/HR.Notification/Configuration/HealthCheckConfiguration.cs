namespace HR.Notification.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddNotificationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddMicroserviceHealthChecks("Notification Service", postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
