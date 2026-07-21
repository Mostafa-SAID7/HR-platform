namespace HR.Notification.Features.SendNotification;

using MediatR;
using HR.Notification.Application.Dtos.Notification;

public static class SendNotificationEndpoint
{
    public static async Task<IResult> Handle(
        SendNotificationRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(httpContext.User.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString());
        
        var command = new SendNotificationCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
