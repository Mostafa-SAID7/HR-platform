namespace HR.Recruitment.Features.GetJobPostings;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// Endpoint for retrieving job postings
/// </summary>
public static class GetJobPostingsEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] JobPostingFilterDto filter,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetJobPostingsQuery(filter, Guid.Empty); // TenantId can be extracted from context if needed
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<JobPostingListDto>>.Ok(result));
    }
}
