namespace HR.Recruitment.Features.PublishJobPosting;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// Endpoint for publishing a job posting
/// </summary>
public static class PublishJobPostingEndpoint
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

        var command = new PublishJobPostingCommand(id, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<JobPostingDto>.Ok(result));
    }
}
