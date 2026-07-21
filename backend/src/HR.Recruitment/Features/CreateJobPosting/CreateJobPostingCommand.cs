namespace HR.Recruitment.Features.CreateJobPosting;

/// <summary>
/// Create a new job posting
/// </summary>
public record CreateJobPostingCommand(
    CreateJobPostingRequest Request,
    Guid TenantId) : ICommand<JobPostingDto>;
