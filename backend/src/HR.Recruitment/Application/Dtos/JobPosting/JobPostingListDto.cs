namespace HR.Recruitment.Application.Dtos.JobPosting;

public record JobPostingListDto(
    Guid Id,
    string Title,
    string Department,
    string Status,
    DateTime PostedDate,
    int ApplicationCount);
