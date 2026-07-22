namespace HR.Identity.Features.ChangePassword;

using MediatR;
using HR.Identity.Application.Dtos.UserProfile;
using HR.Common;
using System.Security.Claims;

/// <summary>
/// Endpoint for changing user password
/// </summary>
public static class ChangePasswordEndpoint
{
    /// <summary>
    /// Handle change password request
    /// </summary>
    public static async Task<IResult> Handle(
        ChangePasswordRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ChangePasswordCommand(userId, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<ChangePasswordResponse>.Ok(result));
    }
}
