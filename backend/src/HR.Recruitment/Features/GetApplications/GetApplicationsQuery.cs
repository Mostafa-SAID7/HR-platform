namespace HR.Recruitment.Features.GetApplications;

/// <summary>
/// Get all applications for a job posting
/// </summary>
public record GetApplicationsQuery(
    Guid JobPostingId,
    Guid TenantId) : IQuery<List<JobApplicationDetailDto>>;
