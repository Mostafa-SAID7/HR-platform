namespace HR.Recruitment.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Seed data configuration for InterviewSchedule entity
/// </summary>
public static class InterviewScheduleSeed
{
    /// <summary>
    /// Seeds initial interview schedule data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var interviewId1 = Guid.Parse("00000000-0000-0000-0000-000000000201");
        var interviewId2 = Guid.Parse("00000000-0000-0000-0000-000000000202");

        modelBuilder.Entity<InterviewSchedule>().HasData(
            new
            {
                Id = interviewId1,
                JobApplicationId = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                ScheduledDate = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
                ScheduledEndTime = new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc),
                InterviewType = "Video",
                InterviewerName = "Alice Johnson",
                InterviewerEmail = "alice.johnson@company.com",
                Status = Domain.InterviewSchedule.InterviewStatus.Scheduled,
                MeetingLink = "https://meet.company.com/recruitment-interview-001",
                Location = (string?)null,
                Feedback = (string?)null,
                CreatedAt = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = interviewId2,
                JobApplicationId = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                ScheduledDate = new DateTime(2026, 7, 25, 14, 0, 0, DateTimeKind.Utc),
                ScheduledEndTime = new DateTime(2026, 7, 25, 15, 30, 0, DateTimeKind.Utc),
                InterviewType = "In-Person",
                InterviewerName = "Bob Wilson",
                InterviewerEmail = "bob.wilson@company.com",
                Status = Domain.InterviewSchedule.InterviewStatus.Scheduled,
                MeetingLink = (string?)null,
                Location = "Conference Room A, 3rd Floor",
                Feedback = (string?)null,
                CreatedAt = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
