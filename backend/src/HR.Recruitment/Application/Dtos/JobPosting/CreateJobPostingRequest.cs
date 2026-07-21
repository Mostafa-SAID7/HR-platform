namespace HR.Recruitment.Application.Dtos.JobPosting;

public record CreateJobPostingRequest(
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax);
