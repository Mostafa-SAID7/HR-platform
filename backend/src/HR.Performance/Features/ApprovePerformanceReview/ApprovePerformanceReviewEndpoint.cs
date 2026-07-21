namespace HR.Performance.Features.ApprovePerformanceReview;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Endpoint for approving a performance review
/// </summary>
public static class ApprovePerformanceReviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        ApprovePerformanceReviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ApprovePerformanceReviewCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}
