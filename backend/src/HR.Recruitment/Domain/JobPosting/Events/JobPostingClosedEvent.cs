namespace HR.Recruitment.Domain.JobPosting.Events;

/// <summary>
/// Domain event raised when a job posting is closed
/// </summary>
public record JobPostingClosedEvent(Guid Id, string Title) : IDomainEvent;
