namespace HR.Recruitment.Features.ScheduleInterview;

/// <summary>
/// Schedule an interview for a job application
/// </summary>
public record ScheduleInterviewCommand(
    Guid JobApplicationId,
    ScheduleInterviewRequest Request,
    Guid TenantId) : ICommand<InterviewScheduleDto>;
