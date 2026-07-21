namespace HR.Recruitment.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Seed data configuration for OfferLetter entity
/// </summary>
public static class OfferLetterSeed
{
    /// <summary>
    /// Seeds initial offer letter data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var offerId1 = Guid.Parse("00000000-0000-0000-0000-000000000301");

        modelBuilder.Entity<OfferLetter>().HasData(
            new
            {
                Id = offerId1,
                JobApplicationId = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                CandidateId = Guid.Parse("00000000-0000-0000-0000-000000002001"),
                CreatedDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                ExpiryDate = new DateTime(2026, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                OfferSalary = 140000m,
                Department = "Engineering",
                Position = "Senior Software Engineer",
                ProposedStartDate = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc),
                Status = Domain.OfferLetter.OfferStatus.Pending,
                Terms = "Competitive benefits package including health insurance, 401k matching, and professional development budget.",
                AcceptedDate = (DateTime?)null,
                RejectedDate = (DateTime?)null,
                CreatedAt = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                LastModifiedAt = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
