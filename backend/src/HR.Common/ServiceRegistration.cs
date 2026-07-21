namespace HR.Common;

using System.Reflection;
using HR.Common.Behaviors;
using HR.Common.Mapping;
using HR.Common.Caching;
using HR.Common.Outbox;
using HR.Common.Messaging;
using MediatR;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for registering common services.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register all common services (MediatR, Mapping, Behaviors, etc.).
    /// </summary>
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Add MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        // Add Mapster
        services.AddMapster(assemblies);

        return services;
    }

    /// <summary>
    /// Register Redis for caching.
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        var multiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
        services.AddSingleton(multiplexer);
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Register Unit of Work and repositories.
    /// </summary>
    public static IServiceCollection AddUnitOfWork<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IUnitOfWork>(provider =>
            new UnitOfWork(provider.GetRequiredService<TDbContext>()));

        return services;
    }

    /// <summary>
    /// Register Outbox pattern services.
    /// </summary>
    public static IServiceCollection AddOutboxPattern<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IOutboxService>(provider =>
            new OutboxService(
                provider.GetRequiredService<TDbContext>(),
                provider.GetRequiredService<ICacheService>()));

        return services;
    }

    /// <summary>
    /// Register MassTransit with Kafka transport for event publishing and consuming.
    /// </summary>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        var kafkaSettings = configuration.GetSection("Kafka");
        var brokers = kafkaSettings.GetValue<string>("Brokers") ?? "localhost:9092";

        services.AddMassTransit(busConfigurator =>
        {
            // Register all consumers from specified assemblies
            busConfigurator.AddConsumers(consumerAssemblies);

            // Use in-memory transport for development
            // In production, this would be configured with actual Kafka
            busConfigurator.UsingInMemory((context, inMemoryConfig) =>
            {
                inMemoryConfig.ConfigureEndpoints(context);
            });
        });

        // Register event publisher
        services.AddScoped<IEventPublisher, EventPublisher>();

        // Register DLQ service
        services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();

        return services;
    }

    /// <summary>
    /// Register MassTransit with Kafka and specific topic configuration.
    /// </summary>
    public static IServiceCollection AddKafkaMessagingWithTopics(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IKafkaTopicsConfig> configureTopics,
        params Assembly[] consumerAssemblies)
    {
        services.AddKafkaMessaging(configuration, consumerAssemblies);

        // Store topic configuration for use during saga registration
        var topicConfig = new KafkaTopicsConfig();
        configureTopics(topicConfig);
        services.AddSingleton<IKafkaTopicsConfig>(topicConfig);

        return services;
    }
}

/// <summary>
/// Kafka topics configuration interface.
/// </summary>
public interface IKafkaTopicsConfig
{
    Dictionary<string, TopicConfig> Topics { get; }
}

/// <summary>
/// Kafka topics configuration implementation.
/// </summary>
public class KafkaTopicsConfig : IKafkaTopicsConfig
{
    public Dictionary<string, TopicConfig> Topics { get; } = new();

    public void AddTopic(string topicName, int partitions = 3, int replicationFactor = 1, int retentionMs = 604800000)
    {
        Topics[topicName] = new TopicConfig
        {
            Name = topicName,
            Partitions = partitions,
            ReplicationFactor = replicationFactor,
            RetentionMs = retentionMs
        };
    }
}

/// <summary>
/// Kafka topic configuration.
/// </summary>
public class TopicConfig
{
    public string Name { get; set; } = string.Empty;
    public int Partitions { get; set; } = 3;
    public int ReplicationFactor { get; set; } = 1;
    public int RetentionMs { get; set; } = 604800000; // 7 days
}
