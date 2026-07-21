namespace HR.Recruitment.Domain.InterviewSchedule.Events;

/// <summary>
/// Domain event raised when an interview is scheduled
/// </summary>
public record InterviewScheduledEvent(
    Guid Id,
    Guid CandidateId,
    string CandidateName,
    DateTime ScheduledDate) : IDomainEvent;
