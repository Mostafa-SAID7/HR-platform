namespace HR.Recruitment.Features.ApplyJob;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.JobApplication;

/// <summary>
/// Endpoint for applying to a job posting
/// </summary>
public static class ApplyJobEndpoint
{
    public static async Task<IResult> Handle(
        Guid jobPostingId,
        ApplyJobRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ApplyJobCommand(jobPostingId, request, Guid.Empty);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/applications/{result.Id}", ApiResponse<JobApplicationDto>.Created(result));
    }
}
