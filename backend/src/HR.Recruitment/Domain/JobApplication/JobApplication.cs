namespace HR.Recruitment.Domain.JobApplication;

/// <summary>
/// Job Application entity - represents a candidate's application for a job posting
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
