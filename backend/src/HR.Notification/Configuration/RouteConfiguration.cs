namespace HR.Notification.Configuration;

using HR.Notification.Features.SendNotification;

public static class RouteConfiguration
{
    public static WebApplication MapNotificationRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/notifications").WithTags("Notifications");
        apiGroup.MapPost("/send", SendNotificationEndpoint.Handle).WithName("SendNotification").WithOpenApi().RequireAuthorization();
        return app;
    }
}
