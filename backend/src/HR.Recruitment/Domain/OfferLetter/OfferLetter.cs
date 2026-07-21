namespace HR.Recruitment.Domain.OfferLetter;

using HR.Recruitment.Domain.OfferLetter.Events;

/// <summary>
/// Offer Letter entity - represents a job offer to a candidate
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
