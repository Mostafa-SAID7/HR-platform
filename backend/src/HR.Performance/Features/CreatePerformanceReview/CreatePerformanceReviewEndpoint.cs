namespace HR.Performance.Features.CreatePerformanceReview;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Endpoint for creating a new performance review
/// </summary>
public static class CreatePerformanceReviewEndpoint
{
    public static async Task<IResult> Handle(
        CreatePerformanceReviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreatePerformanceReviewCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/performance/{result.Id}", ApiResponse<PerformanceReviewDetailDto>.Created(result));
    }
}
