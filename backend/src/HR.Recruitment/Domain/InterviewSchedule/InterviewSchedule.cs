namespace HR.Recruitment.Domain.InterviewSchedule;

/// <summary>
/// Interview Schedule entity - represents scheduled interview for a job application
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
