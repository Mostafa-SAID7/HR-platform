namespace HR.Audit.Configuration;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HR.Common;
using HR.Audit.Application.Services;

/// <summary>
/// Extension methods for registering Audit service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Audit service dependencies
    /// </summary>
    public static IServiceCollection AddAuditService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register application services
        services.AddAuditApplicationServices();

        // Register infrastructure services
        services.AddAuditInfrastructureServices(configuration);

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddAuditApplicationServices(
        this IServiceCollection services)
    {
        // Register MediatR for CQRS
        services.AddCommonServices(
            Assembly.GetExecutingAssembly(),
            typeof(Program).Assembly);

        return services;
    }

    /// <summary>
    /// Register infrastructure services (caching, messaging, consumers)
    /// </summary>
    private static IServiceCollection AddAuditInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis cache
        services.AddRedisCache(
            configuration.GetConnectionString("Redis") ?? "localhost:6379");

        // Register Kafka messaging for consuming events
        services.AddKafkaMessaging(configuration, Assembly.GetExecutingAssembly());

        // Register audit event consumer
        services.AddScoped<AuditEventConsumer>();

        return services;
    }
}
