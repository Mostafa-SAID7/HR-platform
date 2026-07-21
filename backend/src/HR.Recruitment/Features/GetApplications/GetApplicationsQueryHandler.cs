namespace HR.Recruitment.Features.GetApplications;

/// <summary>
/// Handler for GetApplicationsQuery
/// </summary>
public class GetApplicationsQueryHandler : IQueryHandler<GetApplicationsQuery, List<JobApplicationDetailDto>>
{
    private readonly RecruitmentDbContext _dbContext;

    public GetApplicationsQueryHandler(RecruitmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<JobApplicationDetailDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        // Get all applications with their interviews
        var applications = await _dbContext.JobApplications
            .Where(x => x.JobPostingId == request.JobPostingId)
            .Include(x => x.InterviewSchedules)
            .OrderByDescending(x => x.AppliedDate)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        return applications.Adapt<List<JobApplicationDetailDto>>();
    }
}
