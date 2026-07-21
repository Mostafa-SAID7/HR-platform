namespace HR.Notification.Features.UpdatePreference;

using MediatR;
using HR.Notification.Application.Dtos.NotificationPreference;

public static class UpdateNotificationPreferenceEndpoint
{
    public static async Task<IResult> Handle(
        UpdateNotificationPreferenceRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(httpContext.User.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(httpContext.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        
        var command = new UpdateNotificationPreferenceCommand(userId, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
