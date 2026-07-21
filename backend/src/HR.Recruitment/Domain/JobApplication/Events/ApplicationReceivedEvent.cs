namespace HR.Recruitment.Domain.JobApplication.Events;

/// <summary>
/// Domain event raised when a job application is received
/// </summary>
public record ApplicationReceivedEvent(
    Guid Id,
    Guid JobPostingId,
    string CandidateName,
    string CandidateEmail) : IDomainEvent;
