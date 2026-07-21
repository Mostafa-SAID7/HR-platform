namespace HR.Recruitment.Features.GetJobPostings;

/// <summary>
/// Get all job postings with filtering and pagination
/// </summary>
public record GetJobPostingsQuery(
    JobPostingFilterDto Filter,
    Guid TenantId) : IQuery<PaginatedResult<JobPostingListDto>>;
