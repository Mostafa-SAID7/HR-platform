namespace HR.Recruitment.Application.Dtos.JobApplication;

/// <summary>
/// DTO representing a job application summary
/// </summary>
public record JobApplicationDto(
    Guid Id,
    Guid JobPostingId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    DateTime AppliedDate,
    string Status,
    int? Rating,
    string? FeedbackNotes);
