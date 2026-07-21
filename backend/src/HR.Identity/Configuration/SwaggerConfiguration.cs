namespace HR.Identity.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddIdentitySwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service", Version = "v1", Description = "Identity and authentication service" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT" });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] } });
        });
        return services;
    }

    public static WebApplication UseIdentitySwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1"); options.RoutePrefix = "swagger"; });
        return app;
    }
}
