namespace HR.Recruitment.Domain.JobPosting.Events;

/// <summary>
/// Domain event raised when a job posting is created
/// </summary>
public record JobPostingCreatedEvent(
    Guid Id,
    string Title,
    string Department,
    List<string> RequiredSkills) : IDomainEvent;
