namespace HR.Recruitment.Features.GetApplications;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.JobApplication;

/// <summary>
/// Endpoint for retrieving job applications
/// </summary>
public static class GetApplicationsEndpoint
{
    public static async Task<IResult> Handle(
        Guid jobPostingId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetApplicationsQuery(jobPostingId, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<List<JobApplicationDetailDto>>.Ok(result));
    }
}
