namespace HR.Identity.Features.RefreshToken;

using MediatR;
using HR.Identity.Application.Dtos.RefreshToken;
using HR.Common;

/// <summary>
/// Endpoint for refreshing authentication tokens
/// </summary>
public static class RefreshTokenEndpoint
{
    /// <summary>
    /// Handle token refresh request
    /// </summary>
    public static async Task<IResult> Handle(
        RefreshTokenRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new RefreshTokenCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<RefreshTokenResponse>.Ok(result));
    }
}
