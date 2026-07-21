namespace HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// DTO representing detailed job posting information with applications
/// </summary>
public record JobPostingDetailDto(
    Guid Id,
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Status,
    DateTime PostedDate,
    DateTime? ClosedDate,
    int ViewCount,
    List<JobApplication.JobApplicationDto> Applications);
