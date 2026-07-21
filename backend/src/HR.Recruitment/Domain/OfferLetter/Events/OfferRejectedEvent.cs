namespace HR.Recruitment.Domain.OfferLetter.Events;

/// <summary>
/// Domain event raised when an offer letter is rejected
/// </summary>
public record OfferRejectedEvent(
    Guid Id,
    Guid CandidateId,
    string Reason) : IDomainEvent;
