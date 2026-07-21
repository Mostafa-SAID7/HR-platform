namespace HR.Recruitment.Application.Dtos.InterviewSchedule;

/// <summary>
/// DTO representing an interview schedule
/// </summary>
public record InterviewScheduleDto(
    Guid Id,
    Guid JobApplicationId,
    DateTime ScheduledDate,
    DateTime ScheduledEndTime,
    string InterviewType,
    string InterviewerName,
    string InterviewerEmail,
    string Status,
    string? MeetingLink,
    string? Location,
    string? Feedback);
