namespace HR.Recruitment.Features.ApplyJob;

/// <summary>
/// Apply for a job position
/// </summary>
public record ApplyJobCommand(
    Guid JobPostingId,
    ApplyJobRequest Request,
    Guid TenantId) : ICommand<JobApplicationDto>;
