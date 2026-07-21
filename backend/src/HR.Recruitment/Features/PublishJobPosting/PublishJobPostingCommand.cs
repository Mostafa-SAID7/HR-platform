namespace HR.Recruitment.Features.PublishJobPosting;

/// <summary>
/// Publish a job posting (change from Draft to Open)
/// </summary>
public record PublishJobPostingCommand(
    Guid JobPostingId,
    Guid TenantId) : ICommand<JobPostingDto>;
