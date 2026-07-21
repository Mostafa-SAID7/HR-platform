namespace HR.Performance.Features.AddPerformanceFeedback;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos;

/// <summary>
/// Endpoint for adding feedback to a performance review
/// </summary>
public static class AddFeedbackEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        AddFeedbackRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new AddPerformanceFeedbackCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}
