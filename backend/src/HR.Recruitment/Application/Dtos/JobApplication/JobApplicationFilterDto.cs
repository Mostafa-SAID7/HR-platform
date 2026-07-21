namespace HR.Recruitment.Application.Dtos.JobApplication;

public record JobApplicationFilterDto(
    Guid JobPostingId,
    int Page = 1,
    int PageSize = 10,
    string? Status = null);
