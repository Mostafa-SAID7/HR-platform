namespace HR.Recruitment.Features.CreateJobPosting;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// Endpoint for creating a new job posting
/// </summary>
public static class CreateJobPostingEndpoint
{
    public static async Task<IResult> Handle(
        CreateJobPostingRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateJobPostingCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/job-postings/{result.Id}", ApiResponse<JobPostingDto>.Created(result));
    }
}
