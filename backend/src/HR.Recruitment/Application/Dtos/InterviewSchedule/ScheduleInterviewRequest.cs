namespace HR.Recruitment.Application.Dtos.InterviewSchedule;

public record ScheduleInterviewRequest(
    DateTime ScheduledDate,
    DateTime ScheduledEndTime,
    string InterviewType,
    string InterviewerName,
    string InterviewerEmail,
    string? MeetingLink,
    string? Location);
