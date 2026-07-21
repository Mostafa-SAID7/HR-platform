namespace HR.Recruitment.Domain.OfferLetter.Events;

/// <summary>
/// Domain event raised when an offer letter is extended
/// </summary>
public record OfferExtendedEvent(
    Guid Id,
    Guid CandidateId,
    decimal OfferSalary) : IDomainEvent;
