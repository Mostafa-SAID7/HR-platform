namespace HR.Payroll.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Payroll.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering Payroll service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Payroll service dependencies
    /// </summary>
    public static IServiceCollection AddPayrollService(
        this IServiceCollection services,
        string connectionString)
    {
        // Register database context
        services.AddPayrollDatabase(connectionString);

        // Register application services
        services.AddPayrollApplicationServices();

        // Register infrastructure services
        services.AddPayrollInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddPayrollDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PayrollDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, PayrollDbContext>();
        services.AddOutboxPattern<PayrollDbContext>();

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddPayrollApplicationServices(
        this IServiceCollection services)
    {
        // Register MediatR for CQRS
        services.AddCommonServices(
            Assembly.GetExecutingAssembly(),
            typeof(Program).Assembly);

        return services;
    }

    /// <summary>
    /// Register infrastructure services (caching, queries, messaging)
    /// </summary>
    private static IServiceCollection AddPayrollInfrastructureServices(
        this IServiceCollection services)
    {
        // Register Redis cache
        services.AddRedisCache("localhost:6379");

        return services;
    }
}
