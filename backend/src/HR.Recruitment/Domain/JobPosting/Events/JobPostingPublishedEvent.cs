namespace HR.Recruitment.Domain.JobPosting.Events;

/// <summary>
/// Domain event raised when a job posting is published
/// </summary>
public record JobPostingPublishedEvent(Guid Id, string Title) : IDomainEvent;
