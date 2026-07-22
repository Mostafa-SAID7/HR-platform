namespace HR.Identity.Features.OAuthLogin;

using MediatR;
using HR.Identity.Application.Dtos;
using HR.Common;

/// <summary>
/// Endpoint for OAuth login
/// </summary>
public static class OAuthLoginEndpoint
{
    /// <summary>
    /// Handle OAuth login request
    /// </summary>
    public static async Task<IResult> Handle(
        OAuthLoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new OAuthLoginCommand(request.ProviderType, request.AccessToken, request.RefreshToken);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(result));
    }
}
