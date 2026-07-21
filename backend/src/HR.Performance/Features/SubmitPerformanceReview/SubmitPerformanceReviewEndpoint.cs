namespace HR.Performance.Features.SubmitPerformanceReview;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Endpoint for submitting a performance review
/// </summary>
public static class SubmitPerformanceReviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        SubmitPerformanceReviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SubmitPerformanceReviewCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}
