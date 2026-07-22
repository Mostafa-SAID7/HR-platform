namespace HR.Identity.Configuration;

using HR.Identity.Features.Login;
using HR.Identity.Features.Register;
using HR.Identity.Features.RefreshToken;
using HR.Identity.Features.ChangePassword;
using HR.Identity.Features.Profile;

public static class RouteConfiguration
{
    public static WebApplication MapIdentityRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/identity").WithTags("Identity");

        // Authentication endpoints
        apiGroup.MapPost("/login", LoginEndpoint.Handle)
            .WithName("Login")
            .WithOpenApi()
            .AllowAnonymous();

        apiGroup.MapPost("/register", RegisterEndpoint.Handle)
            .WithName("Register")
            .WithOpenApi()
            .AllowAnonymous();

        apiGroup.MapPost("/refresh-token", RefreshTokenEndpoint.Handle)
            .WithName("RefreshToken")
            .WithOpenApi()
            .RequireAuthorization();

        // User profile endpoints
        apiGroup.MapGet("/profile", GetUserProfileEndpoint.Handle)
            .WithName("GetProfile")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/change-password", ChangePasswordEndpoint.Handle)
            .WithName("ChangePassword")
            .WithOpenApi()
            .RequireAuthorization();

        return app;
    }
}
