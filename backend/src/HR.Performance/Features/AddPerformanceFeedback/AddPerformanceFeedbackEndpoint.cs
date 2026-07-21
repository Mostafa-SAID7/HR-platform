namespace HR.Performance.Features.AddPerformanceFeedback;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Endpoint for adding performance feedback
/// </summary>
public static class AddPerformanceFeedbackEndpoint
{
    public static async Task<IResult> Handle(
        Guid performanceReviewId,
        AddPerformanceFeedbackRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new AddPerformanceFeedbackCommand(performanceReviewId, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/performance/feedback/{result.Id}", ApiResponse<PerformanceFeedbackDetailDto>.Created(result));
    }
}
