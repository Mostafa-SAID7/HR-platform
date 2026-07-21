namespace HR.Identity.Configuration;

using HR.Identity.Features.Login;
using HR.Identity.Features.Register;

public static class RouteConfiguration
{
    public static WebApplication MapIdentityRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/identity").WithTags("Identity");
        apiGroup.MapPost("/login", LoginEndpoint.Handle).WithName("Login").WithOpenApi();
        apiGroup.MapPost("/register", RegisterEndpoint.Handle).WithName("Register").WithOpenApi();
        return app;
    }
}
