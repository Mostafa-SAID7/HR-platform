namespace HR.Notification.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Notification.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationService(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, NotificationDbContext>();
        services.AddOutboxPattern<NotificationDbContext>();
        services.AddCommonServices(Assembly.GetExecutingAssembly(), typeof(Program).Assembly);
        services.AddRedisCache("localhost:6379");
        return services;
    }
}
