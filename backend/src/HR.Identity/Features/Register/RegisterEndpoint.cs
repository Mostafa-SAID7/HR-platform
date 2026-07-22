namespace HR.Identity.Features.Register;

using MediatR;
using HR.Identity.Application.Dtos.Register;
using HR.Common;

/// <summary>
/// Endpoint for user registration
/// </summary>
public static class RegisterEndpoint
{
    /// <summary>
    /// Handle registration request
    /// </summary>
    public static async Task<IResult> Handle(
        RegisterRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        
        // For registration, use default tenant if not in claims (public endpoint)
        var tenantId = tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tid)
            ? tid
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new RegisterCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/identity/profile/{result.UserId}", ApiResponse<RegisterResponse>.Created(result));
    }
}
