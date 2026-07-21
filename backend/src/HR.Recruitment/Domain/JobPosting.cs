namespace HR.Recruitment.Domain;

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

/// <summary>
/// Job Application entity
/// </summary>
public class JobApplication : BaseEntity
{
    public Guid JobPostingId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = null!;
    public string CandidateEmail { get; set; } = null!;
    public string CandidatePhone { get; set; } = null!;
    public string Resume { get; set; } = null!; // URL or base64 content
    public string CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public int? Rating { get; set; } // 1-5 scale
    public string? FeedbackNotes { get; set; }
    public List<InterviewSchedule> InterviewSchedules { get; set; } = [];

    public static JobApplication Create(
        Guid jobPostingId,
        Guid candidateId,
        string candidateName,
        string candidateEmail,
        string candidatePhone,
        string resume,
        string? coverLetter = null)
    {
        if (string.IsNullOrWhiteSpace(candidateName))
            throw new DomainException("Candidate name is required");
        if (string.IsNullOrWhiteSpace(candidateEmail))
            throw new DomainException("Candidate email is required");
        if (string.IsNullOrWhiteSpace(resume))
            throw new DomainException("Resume is required");

        return new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPostingId,
            CandidateId = candidateId,
            CandidateName = candidateName,
            CandidateEmail = candidateEmail,
            CandidatePhone = candidatePhone,
            Resume = resume,
            CoverLetter = coverLetter ?? string.Empty,
            Status = ApplicationStatus.Submitted,
            AppliedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(ApplicationStatus newStatus, string? notes = null)
    {
        Status = newStatus;
        FeedbackNotes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetRating(int rating, string notes)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5");

        Rating = rating;
        FeedbackNotes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Interview Schedule entity
/// </summary>
public class InterviewSchedule : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public string InterviewType { get; set; } = null!; // Phone, Video, In-Person
    public string InterviewerName { get; set; } = null!;
    public string InterviewerEmail { get; set; } = null!;
    public InterviewStatus Status { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Feedback { get; set; }

    public static InterviewSchedule Create(
        Guid jobApplicationId,
        DateTime scheduledDate,
        DateTime scheduledEndTime,
        string interviewType,
        string interviewerName,
        string interviewerEmail,
        string? meetingLink = null,
        string? location = null)
    {
        if (scheduledDate >= scheduledEndTime)
            throw new DomainException("Interview start time must be before end time");

        if (scheduledDate <= DateTime.UtcNow)
            throw new DomainException("Interview cannot be scheduled in the past");

        return new InterviewSchedule
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            ScheduledDate = scheduledDate,
            ScheduledEndTime = scheduledEndTime,
            InterviewType = interviewType,
            InterviewerName = interviewerName,
            InterviewerEmail = interviewerEmail,
            Status = InterviewStatus.Scheduled,
            MeetingLink = meetingLink,
            Location = location,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void CompleteInterview(string feedback)
    {
        Status = InterviewStatus.Completed;
        Feedback = feedback;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void CancelInterview(string reason)
    {
        Status = InterviewStatus.Cancelled;
        Feedback = reason;
        LastModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Offer Letter entity
/// </summary>
public class OfferLetter : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal OfferSalary { get; set; }
    public string Department { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime ProposedStartDate { get; set; }
    public OfferStatus Status { get; set; }
    public string? Terms { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? RejectedDate { get; set; }

    public static OfferLetter Create(
        Guid jobApplicationId,
        Guid candidateId,
        decimal offerSalary,
        string department,
        string position,
        DateTime proposedStartDate,
        string? terms = null)
    {
        if (offerSalary <= 0)
            throw new DomainException("Offer salary must be greater than zero");

        if (proposedStartDate <= DateTime.UtcNow.AddDays(1))
            throw new DomainException("Proposed start date must be at least 2 days from now");

        return new OfferLetter
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            CandidateId = candidateId,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30), // 30-day offer validity
            OfferSalary = offerSalary,
            Department = department,
            Position = position,
            ProposedStartDate = proposedStartDate,
            Status = OfferStatus.Pending,
            Terms = terms,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AcceptOffer()
    {
        if (Status != OfferStatus.Pending)
            throw new DomainException("Only pending offers can be accepted");

        if (DateTime.UtcNow > ExpiryDate)
            throw new DomainException("Offer has expired");

        Status = OfferStatus.Accepted;
        AcceptedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RejectOffer(string reason)
    {
        if (Status != OfferStatus.Pending)
            throw new DomainException("Only pending offers can be rejected");

        Status = OfferStatus.Rejected;
        RejectedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
}

// Enums
public enum JobPostingStatus
{
    Draft = 0,
    Open = 1,
    Closed = 2,
    Archived = 3
}

public enum ApplicationStatus
{
    Submitted = 0,
    Shortlisted = 1,
    Rejected = 2,
    Selected = 3,
    Declined = 4,
    OfferExtended = 5
}

public enum InterviewStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    Postponed = 3
}

public enum OfferStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Expired = 3
}

// Domain Events
public record JobPostingCreatedEvent(Guid Id, string Title, string Department, List<string> RequiredSkills) : IDomainEvent;
public record JobPostingPublishedEvent(Guid Id, string Title) : IDomainEvent;
public record JobPostingClosedEvent(Guid Id, string Title) : IDomainEvent;
public record ApplicationReceivedEvent(Guid Id, Guid JobPostingId, string CandidateName, string CandidateEmail) : IDomainEvent;
public record ApplicationShortlistedEvent(Guid Id, Guid CandidateId, string CandidateName) : IDomainEvent;
public record InterviewScheduledEvent(Guid Id, Guid CandidateId, string CandidateName, DateTime ScheduledDate) : IDomainEvent;
public record OfferExtendedEvent(Guid Id, Guid CandidateId, decimal OfferSalary) : IDomainEvent;
public record OfferAcceptedEvent(Guid Id, Guid CandidateId, string CandidateName) : IDomainEvent;
