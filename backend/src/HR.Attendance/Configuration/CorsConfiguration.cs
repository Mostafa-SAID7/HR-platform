namespace HR.Attendance.Configuration;

using Microsoft.Extensions.DependencyInjection;

public static class CorsConfiguration
{
    public static IServiceCollection AddAttendanceCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowGateway", policyBuilder =>
            {
                policyBuilder
                    .WithOrigins("http://localhost:5000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        return services;
    }
}
