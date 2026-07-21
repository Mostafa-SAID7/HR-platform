namespace HR.Recruitment.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Seed data configuration for JobApplication entity
/// </summary>
public static class JobApplicationSeed
{
    /// <summary>
    /// Seeds initial job application data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var applicationId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        var applicationId2 = Guid.Parse("00000000-0000-0000-0000-000000000102");

        modelBuilder.Entity<JobApplication>().HasData(
            new
            {
                Id = applicationId1,
                JobPostingId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CandidateId = Guid.Parse("00000000-0000-0000-0000-000000002001"),
                CandidateName = "John Smith",
                CandidateEmail = "john.smith@example.com",
                CandidatePhone = "+1-555-0101",
                Resume = "https://storage.example.com/resumes/john-smith.pdf",
                CoverLetter = "Passionate software engineer with 8 years of experience...",
                Status = Domain.JobApplication.ApplicationStatus.Shortlisted,
                AppliedDate = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                Rating = 4,
                FeedbackNotes = "Strong technical skills, good communication",
                CreatedAt = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = applicationId2,
                JobPostingId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CandidateId = Guid.Parse("00000000-0000-0000-0000-000000002002"),
                CandidateName = "Jane Doe",
                CandidateEmail = "jane.doe@example.com",
                CandidatePhone = "+1-555-0102",
                Resume = "https://storage.example.com/resumes/jane-doe.pdf",
                CoverLetter = "Experienced engineer with cloud architecture expertise...",
                Status = Domain.JobApplication.ApplicationStatus.Submitted,
                AppliedDate = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                Rating = (int?)null,
                FeedbackNotes = (string?)null,
                CreatedAt = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
