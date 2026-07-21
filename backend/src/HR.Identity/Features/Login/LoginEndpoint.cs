namespace HR.Identity.Features.Login;

using MediatR;
using HR.Identity.Application.Dtos.Login;

public static class LoginEndpoint
{
    public static async Task<IResult> Handle(
        LoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
