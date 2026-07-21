namespace HR.Recruitment.Application.Dtos.JobPosting;

public record UpdateJobPostingRequest(
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax);
