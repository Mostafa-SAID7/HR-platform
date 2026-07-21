namespace HR.Recruitment.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Seed data configuration for JobPosting aggregate
/// </summary>
public static class JobPostingSeed
{
    /// <summary>
    /// Seeds initial job posting data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var jobPostingId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var jobPostingId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        modelBuilder.Entity<JobPosting>().HasData(
            new
            {
                Id = jobPostingId1,
                Title = "Senior Software Engineer",
                Description = "We are seeking an experienced senior software engineer with expertise in cloud technologies and microservices architecture.",
                Department = "Engineering",
                Status = Domain.JobPosting.JobPostingStatus.Open,
                PostedDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                ClosedDate = (DateTime?)null,
                ViewCount = 45,
                SalaryMin = 120000m,
                SalaryMax = 160000m,
                CreatedByUserId = Guid.Parse("00000000-0000-0000-0000-000000001001"),
                CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = jobPostingId2,
                Title = "Product Manager",
                Description = "Looking for an experienced product manager to lead our product strategy and roadmap.",
                Department = "Product",
                Status = Domain.JobPosting.JobPostingStatus.Open,
                PostedDate = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                ClosedDate = (DateTime?)null,
                ViewCount = 32,
                SalaryMin = 100000m,
                SalaryMax = 140000m,
                CreatedByUserId = Guid.Parse("00000000-0000-0000-0000-000000001001"),
                CreatedAt = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
