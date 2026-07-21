namespace HR.Analytics.Configuration;

using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Analytics.Infrastructure.Persistence;
using HR.Analytics.Application.Services;

/// <summary>
/// Extension methods for registering Analytics service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Analytics service dependencies
    /// </summary>
    public static IServiceCollection AddAnalyticsService(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        // Register database context
        services.AddAnalyticsDatabase(connectionString);

        // Register application services
        services.AddAnalyticsApplicationServices();

        // Register infrastructure services
        services.AddAnalyticsInfrastructureServices(configuration);

        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddAnalyticsDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, AnalyticsDbContext>();
        services.AddOutboxPattern<AnalyticsDbContext>();

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddAnalyticsApplicationServices(
        this IServiceCollection services)
    {
        // Register MediatR for CQRS
        services.AddCommonServices(
            Assembly.GetExecutingAssembly(),
            typeof(Program).Assembly);

        return services;
    }

    /// <summary>
    /// Register infrastructure services (caching, Elasticsearch, messaging)
    /// </summary>
    private static IServiceCollection AddAnalyticsInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis cache
        services.AddRedisCache(
            configuration.GetConnectionString("Redis") ?? "localhost:6379");

        // Register Elasticsearch
        var elasticsearchUri = configuration["ConnectionStrings:Elasticsearch"] ?? "http://localhost:9200";
        var client = new ElasticsearchClient(new Uri(elasticsearchUri));
        services.AddSingleton(client);
        services.AddScoped<IElasticsearchService, ElasticsearchService>();

        // Register Kafka messaging
        services.AddKafkaMessaging(configuration, Assembly.GetExecutingAssembly());

        return services;
    }
}
