namespace HR.Employee.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddEmployeeHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddMicroserviceHealthChecks("Employee Service", postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
