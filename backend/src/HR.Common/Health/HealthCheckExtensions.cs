namespace HR.Common.Health;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Extensions for health check configuration.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Add common health checks for microservices.
    /// </summary>
    public static IHealthChecksBuilder AddMicroserviceHealthChecks(
        this IHealthChecksBuilder builder,
        string serviceName,
        string? postgresConnectionString = null,
        string? redisConnectionString = null,
        string? kafkaHost = null)
    {
        builder.AddCheck("service", new ServiceHealthCheck(serviceName));

        if (!string.IsNullOrEmpty(postgresConnectionString))
        {
            builder.AddNpgSql(postgresConnectionString, name: "PostgreSQL");
        }

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            builder.AddRedis(redisConnectionString, name: "Redis");
        }

        return builder;
    }
}

/// <summary>
/// Health check for service availability.
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly string _serviceName;

    public ServiceHealthCheck(string serviceName)
    {
        _serviceName = serviceName;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthCheckResult = HealthCheckResult.Healthy($"{_serviceName} is running");
        return Task.FromResult(healthCheckResult);
    }
}
