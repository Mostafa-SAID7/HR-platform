namespace HR.Recruitment.Application.Dtos.JobApplication;

/// <summary>
/// DTO representing detailed job application information
/// </summary>
public record JobApplicationDetailDto(
    Guid Id,
    Guid JobPostingId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    string Resume,
    string CoverLetter,
    DateTime AppliedDate,
    string Status,
    int? Rating,
    string? FeedbackNotes,
    List<InterviewSchedule.InterviewScheduleDto> InterviewSchedules);
