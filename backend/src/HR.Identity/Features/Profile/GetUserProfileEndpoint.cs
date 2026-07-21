namespace HR.Identity.Features.Profile;

using MediatR;

public static class GetUserProfileEndpoint
{
    public static async Task<IResult> Handle(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var query = new GetUserProfileQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
