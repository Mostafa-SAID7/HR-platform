namespace HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// DTO representing a job posting summary
/// </summary>
public record JobPostingDto(
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
    int ApplicationCount);
