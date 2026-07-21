namespace HR.Performance.Features.SubmitPerformanceReview;

using MediatR;
using HR.Common.Domain;

/// <summary>
/// Endpoint for submitting a performance review
/// </summary>
public static class SubmitReviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SubmitPerformanceReviewCommand(id, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Performance review submitted successfully"));
    }
}
