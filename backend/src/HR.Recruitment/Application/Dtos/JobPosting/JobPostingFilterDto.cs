namespace HR.Recruitment.Application.Dtos.JobPosting;

public record JobPostingFilterDto(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Department = null,
    string? Status = null);
