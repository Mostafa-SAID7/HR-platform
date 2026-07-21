namespace HR.Recruitment.Domain.JobApplication.Events;

/// <summary>
/// Domain event raised when a job application is shortlisted
/// </summary>
public record ApplicationShortlistedEvent(
    Guid Id,
    Guid CandidateId,
    string CandidateName) : IDomainEvent;
