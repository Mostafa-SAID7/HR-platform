namespace HR.Performance.Features.GetPerformanceReviews;

using MediatR;
using HR.Common.Domain;
using HR.Performance.Application.Dtos;

/// <summary>
/// Endpoint for getting performance reviews with filtering and pagination
/// </summary>
public static class GetReviewsEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] PerformanceFilterDto filter,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPerformanceReviewsQuery(filter, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<PerformanceReviewListDto>>.Ok(result));
    }
}
