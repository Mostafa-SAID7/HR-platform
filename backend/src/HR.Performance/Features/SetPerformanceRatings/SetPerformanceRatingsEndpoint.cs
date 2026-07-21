namespace HR.Performance.Features.SetPerformanceRatings;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Endpoint for setting performance ratings
/// </summary>
public static class SetPerformanceRatingsEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        SetPerformanceRatingsRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new SetPerformanceRatingsCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}
