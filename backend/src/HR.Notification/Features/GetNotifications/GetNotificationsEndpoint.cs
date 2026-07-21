namespace HR.Notification.Features.GetNotifications;

using MediatR;
using HR.Notification.Application.Dtos.Notification;

public static class GetNotificationsEndpoint
{
    public static async Task<IResult> Handle(
        IMediator mediator,
        HttpContext httpContext,
        [FromQuery] Guid? recipientId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse(httpContext.User.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString());
        
        var filter = new NotificationFilterDto(recipientId, status, page, pageSize);
        var query = new GetNotificationsQuery(filter, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
