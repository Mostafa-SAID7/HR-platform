namespace HR.Performance.Features.GetPerformanceReviewById;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos;

/// <summary>
/// Endpoint for getting a performance review by ID
/// </summary>
public static class GetReviewByIdEndpoint
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

        var query = new GetPerformanceReviewByIdQuery(id, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(result));
    }
}
