namespace HR.Identity.Features.RequestOtp;

using MediatR;
using HR.Identity.Application.Dtos;
using HR.Common;

/// <summary>
/// Endpoint for requesting OTP
/// </summary>
public static class RequestOtpEndpoint
{
    /// <summary>
    /// Handle request OTP request
    /// </summary>
    public static async Task<IResult> Handle(
        RequestOtpRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RequestOtpCommand(request.Email, request.PhoneNumber, request.OtpType);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<OtpResponseDto>.Ok(result));
    }
}
