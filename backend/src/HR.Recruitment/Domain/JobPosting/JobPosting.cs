namespace HR.Recruitment.Domain.JobPosting;

using HR.Recruitment.Domain.JobPosting.Events;

/// <summary>
/// Job Posting aggregate root - represents a job opening
/// </summary>
public class JobPosting : AggregateRoot
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Department { get; private set; }
    public List<string> RequiredSkills { get; private set; } = [];
    public decimal? SalaryMin { get; private set; }
    public decimal? SalaryMax { get; private set; }
    public JobPostingStatus Status { get; private set; }
    public DateTime PostedDate { get; private set; }
    public DateTime? ClosedDate { get; private set; }
    public int ViewCount { get; private set; }
    public List<JobApplication> Applications { get; private set; } = [];
    public Guid? CreatedByUserId { get; private set; }

    private JobPosting() { }

    /// <summary>
    /// Create a new job posting
    /// </summary>
    public static JobPosting Create(
        string title,
        string description,
        string department,
        List<string> requiredSkills,
        decimal? salaryMin = null,
        decimal? salaryMax = null,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Job title is required");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Job description is required");
        if (string.IsNullOrWhiteSpace(department))
            throw new DomainException("Department is required");
        if (requiredSkills == null || requiredSkills.Count == 0)
            throw new DomainException("At least one required skill must be specified");

        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Department = department,
            RequiredSkills = requiredSkills,
            SalaryMin = salaryMin,
            SalaryMax = salaryMax,
            Status = JobPostingStatus.Draft,
            PostedDate = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        jobPosting.RaiseDomainEvent(new JobPostingCreatedEvent(
            jobPosting.Id,
            title,
            department,
            requiredSkills));

        return jobPosting;
    }

    /// <summary>
    /// Publish the job posting
    /// </summary>
    public void Publish()
    {
        if (Status != JobPostingStatus.Draft)
            throw new DomainException("Only draft postings can be published");

        Status = JobPostingStatus.Open;
        PostedDate = DateTime.UtcNow;

        RaiseDomainEvent(new JobPostingPublishedEvent(Id, Title));
    }

    /// <summary>
    /// Close the job posting
    /// </summary>
    public void Close()
    {
        if (Status == JobPostingStatus.Closed)
            throw new DomainException("Job posting is already closed");

        Status = JobPostingStatus.Closed;
        ClosedDate = DateTime.UtcNow;

        RaiseDomainEvent(new JobPostingClosedEvent(Id, Title));
    }

    /// <summary>
    /// Update view count
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
    }

    /// <summary>
    /// Update job posting details
    /// </summary>
    public void Update(
        string title,
        string description,
        string department,
        List<string> requiredSkills,
        decimal? salaryMin,
        decimal? salaryMax)
    {
        if (Status != JobPostingStatus.Draft)
            throw new DomainException("Only draft postings can be updated");

        Title = title;
        Description = description;
        Department = department;
        RequiredSkills = requiredSkills;
        SalaryMin = salaryMin;
        SalaryMax = salaryMax;
        LastModifiedAt = DateTime.UtcNow;
    }
}
